using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using Microsoft.Extensions.DependencyInjection;
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther

namespace MathCore.DI;

public static class Compositor
{
    private static FieldBuilder MakeField(this TypeBuilder type, PropertyInfo Property)
    {
        var field_attributes = FieldAttributes.Private;
        if (!Property.CanWrite) field_attributes |= FieldAttributes.InitOnly;
        var field = type.DefineField(
            fieldName: $"_{Property.Name}",
            type: Property.PropertyType,
            attributes: field_attributes);
        
        return field;
    }

    private static void MakeProperty(this TypeBuilder type, PropertyInfo Property, FieldInfo Field)
    {
        var property = type.DefineProperty(
            name: Property.Name,
            attributes: PropertyAttributes.HasDefault,
            returnType: Property.PropertyType,
            parameterTypes: null);

        const MethodAttributes property_method_attributes =
            MethodAttributes.Public |
            MethodAttributes.SpecialName |
            MethodAttributes.HideBySig |
            MethodAttributes.Virtual;

        if (Property.CanRead)
        {
            var get_method = type.DefineMethod(
                name: $"get_{Property.Name}",
                attributes: property_method_attributes,
                returnType: property.PropertyType,
                parameterTypes: Type.EmptyTypes);

            var il = get_method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, Field);
            il.Emit(OpCodes.Ret);

            property.SetGetMethod(get_method);
        }

        if (Property.CanWrite)
        {
            var set_method = type.DefineMethod(
                name: $"set_{Property.Name}",
                attributes: property_method_attributes,
                returnType: null,
                parameterTypes: new [] { property.PropertyType });

            var il = set_method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, Field);
            il.Emit(OpCodes.Ret);

            property.SetSetMethod(set_method);
        }
    }

    private static void MakeConstructor(this TypeBuilder type, IReadOnlyCollection<FieldBuilder> Fields)
    {
        var ctor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Fields.Select(p => p.FieldType).ToArray());
        var ctor_il = ctor.GetILGenerator();
        ctor_il.Emit(OpCodes.Ldarg_0);
        ctor_il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);

        var parameter_index = 1;
        foreach (var field in Fields)
        {
            ctor_il.Emit(OpCodes.Ldarg_0);
            switch (parameter_index++)
            {
                case 1:
                    ctor_il.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    ctor_il.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    ctor_il.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    ctor_il.Emit(OpCodes.Ldarg_S, parameter_index);
                    break;
            }

            ctor_il.Emit(OpCodes.Stfld, field);
        }

        ctor_il.Emit(OpCodes.Ret);
    }

    private static List<FieldBuilder> MakeProperties(this TypeBuilder type, IEnumerable<PropertyInfo> Properties)
    {
        var fields = new List<FieldBuilder>();
        foreach (var property_info in Properties)
        {
            var field = type.MakeField(property_info);
            type.MakeProperty(property_info, field);
            fields.Add(field);
        }

        return fields;
    }

    private static Type MakeImplementation(Type InterfaceType)
    {
        var properties = InterfaceType.GetProperties();

        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("MathCore.Hosting.Emit");

        var composition_type_name = $"MathCore.Hosting.Emit.{InterfaceType.Name.TrimStart('I')}Composition";
        var type = module!.DefineType(
            composition_type_name,
            TypeAttributes.Class,
            typeof(object),
            new[] { InterfaceType });

        type.MakeConstructor(type.MakeProperties(properties));

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

