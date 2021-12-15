using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther

namespace MathCore.DI;

public static class Compositor
{
    private static Type MakeImplementation(Type InterfaceType)
    {
        var properties = InterfaceType.GetProperties();

        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("MathCore.Hosting.Emit");

        var composition_type_name = $"MathCore.Hosting.Emit.{InterfaceType.Name.TrimStart('I')}Composition";
        var type = module!.DefineType(
            composition_type_name, /*TypeAttributes.Public*/
            TypeAttributes.Class,
            typeof(object),
            //null
            new[] { InterfaceType });

        //type.AddInterfaceImplementation(InterfaceType);

        var fields = new List<FieldBuilder>();
        foreach (var property_info in properties)
        {
            var field = type.DefineField(
                fieldName: $"_{property_info.Name}", 
                type: property_info.PropertyType, 
                attributes: FieldAttributes.Private);

            fields.Add(field);

            var property = type.DefineProperty(
                name: property_info.Name,
                attributes: PropertyAttributes.HasDefault,
                returnType: property_info.PropertyType,
                parameterTypes: null);

            const MethodAttributes property_method_attributes =
                MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.HideBySig | 
                MethodAttributes.Virtual;

            if (property_info.CanRead)
            {
                var get_method = type.DefineMethod(
                    name: $"get_{property_info.Name}",
                    attributes: property_method_attributes,
                    returnType: property.PropertyType,
                    parameterTypes: Type.EmptyTypes);

                var il = get_method.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, field);
                il.Emit(OpCodes.Ret);

                property.SetGetMethod(get_method);
            }

            if (property_info.CanWrite)
            {
                var set_method = type.DefineMethod(
                    $"set_{property_info.Name}",
                    MethodAttributes.Public,
                    CallingConventions.ExplicitThis);

                var il = set_method.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, field);
                il.Emit(OpCodes.Ret);

                property.SetSetMethod(set_method);
            }
        }

        var ctor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, properties.Select(p => p.PropertyType).ToArray());
        var ctor_il = ctor.GetILGenerator();
        ctor_il.Emit(OpCodes.Ldarg_0);
        ctor_il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);

        var parameter_index = 1;
        foreach (var field in fields)
        {
            ctor_il.Emit(OpCodes.Ldarg_0);
            switch (parameter_index++)
            {
                case 1: ctor_il.Emit(OpCodes.Ldarg_1); break;
                case 2: ctor_il.Emit(OpCodes.Ldarg_2); break;
                case 3: ctor_il.Emit(OpCodes.Ldarg_3); break;
                default: ctor_il.Emit(OpCodes.Ldarg_S, parameter_index); break;
            }
            ctor_il.Emit(OpCodes.Stfld, field);
        }

        ctor_il.Emit(OpCodes.Ret);

        var implementation_type = type.CreateType();

        return implementation_type;
    }

    public static IServiceCollection AddComposite<TInterface>(this IServiceCollection services, ServiceLifetime ServiceLifetime)
    {
        var interface_type = typeof(TInterface);
        var implementation_type = MakeImplementation(interface_type);

        services.Add(new ServiceDescriptor(interface_type, implementation_type, ServiceLifetime));

        return services;
    }
}

