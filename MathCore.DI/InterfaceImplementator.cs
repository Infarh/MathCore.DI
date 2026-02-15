using System.Reflection.Emit;

// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleLiteral

namespace MathCore.DI;

/// <summary>Реализует методы для создания реализации интерфейса в рантайме с помощью Reflection.Emit</summary>
internal static class InterfaceImplementator
{
    // https://alistairevans.co.uk/2020/11/01/detecting-init-only-properties-with-reflection-in-c-9/
    /// <param name="property">Информация об исследуемом свойстве</param>
    extension(PropertyInfo property)
    {
        /// <summary>Определение - является ли свойство доступным только для инициализации</summary>
        /// <returns>Истина, если свойство имеет модификатор <c>init</c></returns>
        public bool IsInitOnly()
        {
            if (!property.CanWrite) return false;

            var set_method = property.SetMethod.NotNull();

            // Get the modifiers applied to the return parameter.
            var method_return_parameter_modifiers = set_method.ReturnParameter!.GetRequiredCustomModifiers();

            // Init-only properties are marked with the IsExternalInit type.
            return method_return_parameter_modifiers.Any(type => type.FullName == "System.Runtime.CompilerServices.IsExternalInit");
        }
    }

    extension(TypeBuilder type)
    {
        private (FieldBuilder Field, PropertyBuilder Property) MakeProperty(PropertyInfo Property)
        {

            var field = type.MakeField(
                FieldName: $"_{Property.Name}",
                PropertyType: Property.PropertyType,
                Readonly: !Property.CanWrite);

            var property = type.DefineProperty(
                name: Property.Name,
                attributes: Property.Attributes,
                returnType: Property.PropertyType,
                parameterTypes: null);

            if (Property.GetMethod is { } get_method_info)
            {
                var get_method = type.DefineMethod(
                    name: get_method_info.Name,
                    attributes: get_method_info.Attributes & ~MethodAttributes.Abstract,
                    callingConvention: get_method_info.CallingConvention,
                    returnType: property.PropertyType,
                    parameterTypes: Type.EmptyTypes);

                var il = get_method.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, field);
                il.Emit(OpCodes.Ret);

                property.SetGetMethod(get_method);
            }

            if (Property.SetMethod is { } set_method_info)
            {
                var is_init_only = Property.IsInitOnly();
                if (is_init_only)
                    throw new NotSupportedException($"В реализуемом интерфейсе {Property.DeclaringType} свойство {Property.Name} имеет неподдерживаемый модификатор init");

                var set_method = type.DefineMethod(
                    name: set_method_info.Name,
                    attributes: set_method_info.Attributes & ~MethodAttributes.Abstract,
                    callingConvention: set_method_info.CallingConvention,
                    returnType: null,
                    parameterTypes: [property.PropertyType]);

                var il = set_method.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, field);
                il.Emit(OpCodes.Ret);

                //set_method.re

                property.SetSetMethod(set_method);
            }

            return (field, property);
        }

        private FieldBuilder MakeField(string FieldName, Type PropertyType, bool Readonly = false) =>
            type.DefineField(
                fieldName: FieldName,
                type: PropertyType,
                attributes: Readonly
                    ? FieldAttributes.Private | FieldAttributes.InitOnly
                    : FieldAttributes.Private);

        private void MakeConstructor(IReadOnlyCollection<FieldBuilder> Fields)
        {
            var ctor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Fields.Select(p => p.FieldType).ToArray());
            var ctor_il = ctor.GetILGenerator();
            ctor_il.Emit(OpCodes.Ldarg_0);
            ctor_il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);

            byte parameter_index = 1;
            foreach (var field in Fields)
            {
                ctor_il.Emit(OpCodes.Ldarg_0);
                ctor_il.Emit(OpCodes.Ldarg_S, parameter_index++);
                ctor_il.Emit(OpCodes.Stfld, field);
            }

            ctor_il.Emit(OpCodes.Ret);
        }

        private List<FieldBuilder> MakeProperties(IEnumerable<PropertyInfo> Properties) =>
            [.. Properties.Select(PropertyInfo => type.MakeProperty(PropertyInfo).Field)];
    }

    public static Type CreateImplementation(this Type InterfaceType)
    {
        var properties = InterfaceType.GetProperties();

        var assembly = AssemblyBuilder.DefineDynamicAssembly(new(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("MathCore.Hosting.Emit");

        var composition_type_name = $"MathCore.Hosting.Emit.{InterfaceType.Name.TrimStart('I')}Composition";
        var type = module!.DefineType(
            name: composition_type_name,
            attr: TypeAttributes.Class,
            parent: typeof(object),
            interfaces: [InterfaceType]);

        var properties_impl = type.MakeProperties(properties);
        type.MakeConstructor(properties_impl);

        var type_info = type.CreateTypeInfo() ?? throw new InvalidOperationException();

        return type_info.AsType();
    }
}