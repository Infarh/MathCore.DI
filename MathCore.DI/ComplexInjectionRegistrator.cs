using System.Linq.Expressions;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MathCore.DI;

public static class ComplexInjectionRegistrator
{
    /// <summary>Проверка - является ли сервис простым и требует простого режима регистрации</summary>
    /// <param name="type">Проверяемый на простоту тип</param>
    /// <returns>Истина, если </returns>
    private static bool IsSimple(this Type type)
    {
        var fields = type
           .GetFields(BindingFlags.Instance | BindingFlags.Public)
           .Concat(type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
           .Cast<MemberInfo>();

        var properties = type
           .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty)
           .Concat(type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty))
           .Cast<MemberInfo>();

        //if (fields.Concat(properties).Any(member => member.GetCustomAttribute<InjectAttribute>() is not null)) 
        //    return false;

        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
           .Concat(type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
           .Cast<MemberInfo>();

        var ctor_has_inject_attribute = type
           .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
           .Concat(type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic))
           .SelectMany(p => p.GetParameters())
           .Any(p => p.GetCustomAttribute<InjectAttribute>() is not null);

        if (ctor_has_inject_attribute)
            return false;

        //return !methods.Any(m => m.GetCustomAttribute<InjectAttribute>() != null);
        return !fields
           .Concat(properties)
           .Concat(methods)
           .Any(member => member.GetCustomAttribute<InjectAttribute>() is not null);
    }

    /// <summary>Формирование дескриптора регистрации сервиса с инициализацией его через свойства, поля, методы и конструктор</summary>
    /// <param name="Service">Тип регистрируемого сервиса (интерфейс)</param>
    /// <param name="Implementation">Реализация сервиса</param>
    /// <param name="Mode">Режим регистрации</param>
    /// <returns>Дескриптор регистрации сервиса</returns>
    /// <exception cref="InvalidOperationException">В случае ошибок формирования делегата фабрики сервиса</exception>
    private static ServiceDescriptor CreateDescriptor(this Type Service, Type? Implementation, ServiceLifetime Mode)
    {
        var service_type = Implementation ?? Service;

        const BindingFlags inst_public = BindingFlags.Instance | BindingFlags.Public;
        const BindingFlags inst_no_public = BindingFlags.Instance | BindingFlags.NonPublic;

        var ctors = service_type.GetConstructors(inst_public)
           .Concat(service_type.GetConstructors(inst_no_public))
           .OrderByDescending(c => c.GetParameters().Length)
           .ToArray();

        // Ищем конструктор с самым большим числом параметров публичный и приватный
        var ctor = ctors.FirstOrDefault(c => c.GetParameters().Any(p => p.GetCustomAttribute<InjectAttribute>() is not null))
            ?? ctors[0];

        var sp = Expression.Parameter(typeof(IServiceProvider), "sp"); // Провайдер сервисов

        // Определение метода (расширения) object IServiceProvider.GetRequiredService(Type)
        var get_required_service = typeof(ServiceProviderServiceExtensions)
               .GetMethod(
                    nameof(ServiceProviderServiceExtensions.GetRequiredService),
                    new[] { typeof(IServiceProvider), typeof(Type) })
            ?? throw new InvalidOperationException("Не найден метод-расширения object ServiceProviderServiceExtensions.GetRequiredService(this IServiceProvider, Type)");

        var get_service = typeof(IServiceProvider).GetMethod(nameof(IServiceProvider.GetService), new[] { typeof(Type) })
            ?? throw new InvalidOperationException("Не найден метод-расширения GetService(Type) для интерфейса IServiceProvider"); ;

        // Определяем выражения для каждого из параметров конструктора
        // Выражение должно обращаться к провайдеру сервисов и требовать у него объект указанного типа
        var ctor_parameters = ctor.GetParameters()
           .Select(parameter => (parameter, parameter.GetCustomAttribute<InjectAttribute>()))
           .Select(p =>
            {
                var (parameter, inject) = p;
                var required = inject?.Required ?? !parameter.GetCustomAttributes().Any(a => a.GetType().Name == "NullableAttribute");
                var parameter_type = parameter.ParameterType;               // Тип текущего параметра конструктора
                var type = Expression.Constant(parameter_type);             //    превращаем в выражение
                // Вызываем sp.GetRequiredService(parameter_type)
                var call = required
                    ? Expression.Call(get_required_service, sp, type)
                    : Expression.Call(sp, get_service, type);
                return Expression.Convert(call, parameter_type);            // Полученный объект приводим к parameter_type
            });

        // Ищем все свойства публичные и приватные, принадлежащие экземпляру объекта
        var properties = service_type.GetProperties(inst_public | BindingFlags.SetProperty)
           .Concat(service_type.GetProperties(inst_no_public | BindingFlags.SetProperty))
           .Select(property => (property, Inject: property.GetCustomAttribute<InjectAttribute>()))
           .Where(p => p.Inject is not null)
           // Выбираем те свойства, у которых есть атрибут [Inject]
           .Select(p =>
            {
                var (property, inject) = p;
                var required = inject?.Required ?? !property.GetCustomAttributes().Any(a => a.GetType().Name == "NullableAttribute");
                var property_type = property.PropertyType;               // Тип свойства
                var type = Expression.Constant(property_type);           //    превращаем в выражение
                // Вызываем sp.GetRequiredService(property_type)
                var obj = required
                    ? Expression.Call(get_required_service, sp, type)
                    : Expression.Call(sp, get_service, type);
                var value = Expression.Convert(obj, property_type);      // Полученный объект приводим к property_type
                return (MemberBinding)Expression.Bind(property, value);  // Формируем выражение, выполняющее привязку полученного значения к свойству
            })
           .ToArray();

        // Ищем все поля публичные и приватные, принадлежащие экземпляру объекта
        var fields = service_type.GetFields(inst_public)
           .Concat(service_type.GetFields(inst_no_public))
           .Select(field => (property: field, Inject: field.GetCustomAttribute<InjectAttribute>()))
           .Where(p => p.Inject is not null)
           .Select(f =>
            {
                var (field, inject) = f;
                var required = inject?.Required ?? !field.GetCustomAttributes().Any(a => a.GetType().Name == "NullableAttribute");
                var field_type = field.FieldType;                       // Тип поля
                var type = Expression.Constant(field_type);             //    превращаем в выражение
                // Вызываем sp.GetRequiredService(property_type)
                var obj = required
                    ? Expression.Call(get_required_service, sp, type)
                    : Expression.Call(sp, get_service, type);
                var value = Expression.Convert(obj, field_type);        // Полученный объект приводим к property_type
                return (MemberBinding)Expression.Bind(field, value);    // Формируем выражение, выполняющее привязку полученного значения к полю
            })
           .ToArray();

        var member_inits = fields.Concat(properties).ToArray();

        // Формируем выражение вызова конструктора
        var ctor_expr = Expression.New(ctor, ctor_parameters);

        // Формируем выражение формирования экземпляра объекта
        Expression instance = member_inits.Length > 0
            ? Expression.MemberInit(ctor_expr, member_inits)    // Если есть выражения инициализации полей, или свойств, то добавляем их
            : ctor_expr;                                        // Иначе выбираем просто вызов конструктора

        // Ищем методы-инициализаторы для выполнения внедрения зависимостей через них
        var result = Expression.Variable(Service, "result");
        // Ищем все публичные и непубличные методы экземпляра
        var methods = service_type.GetMethods(inst_public)
           .Concat(service_type.GetMethods(inst_no_public))
           .Where(InitMethod => InitMethod.GetCustomAttribute<InjectAttribute>() != null) // где есть атрибут [Inject]
           .Select(InitMethod =>
           {
               // Определяем все параметры выбранного метода
               var parameters = InitMethod
                  .GetParameters()
                  .Select(parameter =>
                   {
                       var inject = parameter.GetCustomAttribute<InjectAttribute>();
                       var required = inject?.Required ?? !parameter.GetCustomAttributes().Any(a => a.GetType().Name == "NullableAttribute");
                       var parameter_type = parameter.ParameterType;     // Определяем тип параметра
                       var type = Expression.Constant(parameter_type);   //   формируем из него выражение
                       // Формируем вызов к провайдеру сервисов для получения экземпляра указанного типа
                       var obj = required
                           ? Expression.Call(get_required_service, sp, type)
                           : Expression.Call(sp, get_service, type);
                       return Expression.Convert(obj, parameter_type);   // Приводим тип к типу параметра
                  });

               // Формируем выражение вызова данного метода с передачей ему полного набора параметров
               return (Expression)Expression.Call(result, InitMethod, parameters);
           })
           .ToArray();

        // Итого:
        // - был выбран конструктор и сформировано выражение его вызова
        // - были определены свойства, помеченные атрибутом [Inject] и сформированы методы их инициализации с запросом значений у IServiceProvider
        // - были определены поля, помеченные атрибутом [Inject] и сформированы методы их инициализации с запросом значений у IServiceProvider
        // - определены методы помеченные атрибутом [Inject] и сформированы выражения их вызовов с передачей им параметров, значений которых извлекаются из IServiceProvider
        // Теперь необходимо всё скомпоновать в единое выражение

        // Если есть методы инициализации
        var body = methods.Length > 0
            ? Expression.Block(     // формируем блок с последовательностью вызовов этих методов
                new[] { result },                                               // определяем переменные внутри блока - объект сервиса
                Expression.Assign(result, instance),                            // присваиваем созданной переменной объект сервиса, полученный после вызова выражения формирования экземпляра этого типа
                methods.Length == 1 ? methods[0] : Expression.Block(methods),   // Если метод инициализации один, то вставляем его непосредственно в блок. Иначе - формируем новый вложенный блок, содержащий весь набор методов-инициализаторов
                result) // Последним элементом блока указываем возвращаемое значение
            : instance; // Если методов инициализации нет, то просто берём выражение формирования нового экземпляра класса сервиса

        // Формируем выражение инициализации экземпляра сервиса
        var factory_expr = Expression.Lambda<Func<IServiceProvider, object>>(body, sp);
        // компилируем его в делегат
        var factory = factory_expr.Compile()
            ?? throw new InvalidOperationException("Не удалось выполнить сборку выражения инициализации сервиса");

        return new ServiceDescriptor(Service, factory, Mode);
    }

    /// <summary>Добавить сервис с возможностями внедрения зависимости через поля/свойства и методы</summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="Service">Регистрируемый сервис</param>
    /// <param name="Implementation">Реализация сервиса</param>
    /// <param name="Mode">Режим регистрации</param>
    /// <returns>Коллекция сервиса с добавленным в неё регистрируемым сервисом</returns>
    /// <exception cref="InvalidOperationException">В случае если указанная реализация сервиса не реализует указанного интерфейса</exception>
    public static IServiceCollection AddService(this IServiceCollection services, Type Service, Type? Implementation, ServiceLifetime Mode)
    {
        var service_type = Implementation is null
            ? Service
            : Service.IsAssignableFrom(Implementation)
                ? Implementation
                : throw new InvalidOperationException($"Тип {Implementation} не реализует {Service}");

        if (service_type.IsSimple())
            return services.AddSimple(Service, Implementation, Mode);

        services.TryAdd(Service.CreateDescriptor(Implementation, Mode));

        return services;
    }

    /// <summary>Упрощённая регистрация сервиса</summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="Service">Регистрируемый сервис</param>
    /// <param name="Implementation">Реализация сервиса</param>
    /// <param name="Mode">Режим регистрации</param>
    /// <returns>Коллекция сервиса с добавленным в неё регистрируемым сервисом</returns>
    private static IServiceCollection AddSimple(this IServiceCollection services, Type Service, Type? Implementation, ServiceLifetime Mode)
    {
        var descriptor = Implementation is null
            ? new ServiceDescriptor(Service, Service, Mode)
            : new ServiceDescriptor(Service, Implementation, Mode);

        services.TryAdd(descriptor);
        return services;
    }
}