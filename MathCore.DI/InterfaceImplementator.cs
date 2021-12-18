using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleLiteral

namespace MathCore.DI;

internal static class InterfaceImplementator
{
    private static (FieldBuilder Field, PropertyBuilder Property) MakeProperty(this TypeBuilder type, PropertyInfo Property) =>
        type.MakeProperty(
            PropertyName: Property.Name,
            PropertyType: Property.PropertyType,
            CanWrite: Property.CanWrite, 
            CanRead: Property.CanRead);

    private static (FieldBuilder Field, PropertyBuilder Property) MakeProperty(
        this TypeBuilder type,
        string PropertyName,
        Type PropertyType,
        bool CanWrite = true,
        bool CanRead = true)
    {
        var field = type.MakeField($"_{PropertyName}", PropertyType, !CanWrite);

        var property = type.DefineProperty(
            name: PropertyName,
            attributes: PropertyAttributes.HasDefault,
            returnType: PropertyType,
            parameterTypes: null);

        const MethodAttributes property_method_attributes =
            MethodAttributes.Public |
            MethodAttributes.SpecialName |
            MethodAttributes.HideBySig |
            MethodAttributes.Virtual;

        if (CanRead)
        {
            var get_method = type.DefineMethod(
                name: $"get_{PropertyName}",
                attributes: property_method_attributes,
                returnType: property.PropertyType,
                parameterTypes: Type.EmptyTypes);

            var il = get_method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, field);
            il.Emit(OpCodes.Ret);

            property.SetGetMethod(get_method);
        }

        if (CanWrite)
        {
            var set_method = type.DefineMethod(
                name: $"set_{PropertyName}",
                attributes: property_method_attributes,
                returnType: null,
                parameterTypes: new[] { property.PropertyType });

            var il = set_method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, field);
            il.Emit(OpCodes.Ret);

            property.SetSetMethod(set_method);
        }

        return (field, property);
    }

    private static FieldBuilder MakeField(this TypeBuilder type, string FieldName, Type PropertyType, bool Readonly = false) =>
        type.DefineField(
            fieldName: FieldName,
            type: PropertyType,
            attributes: Readonly 
                ? FieldAttributes.Private | FieldAttributes.InitOnly 
                : FieldAttributes.Private);

    private static void MakeConstructor(this TypeBuilder type, IReadOnlyCollection<FieldBuilder> Fields)
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

    private static List<FieldBuilder> MakeProperties(this TypeBuilder type, IEnumerable<PropertyInfo> Properties) => 
        Properties
           .Select(property_info => type.MakeProperty(property_info).Field)
           .ToList();

    public static Type CreateImplementation(this Type InterfaceType)
    {
        var properties = InterfaceType.GetProperties();

        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("MathCore.Hosting.Emit");

        var composition_type_name = $"MathCore.Hosting.Emit.{InterfaceType.Name.TrimStart('I')}Composition";
        var type = module!.DefineType(
            name: composition_type_name,
            attr: TypeAttributes.Class,
            parent: typeof(object),
            interfaces: new[] { InterfaceType });

        type.MakeConstructor(type.MakeProperties(properties));

        var type_info = type.CreateTypeInfo() ?? throw new InvalidOperationException();

        return type_info.AsType();
    }
}