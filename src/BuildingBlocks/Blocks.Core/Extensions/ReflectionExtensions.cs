using System.Reflection;

namespace Blocks.Core;

public static class ReflectionExtensions
{
    public static IEnumerable<MemberInfo> GetPropertiesAndFields(this Type @this, BindingFlags flags)
    {
        foreach (var property in @this.GetProperties(flags))
            yield return property;

        foreach (var field in @this.GetFields(flags))
            yield return field;
    }

    public static object GetValue(this MemberInfo @this, object target)
    {
        // switch might be better here for type branching
        if (@this is PropertyInfo property)
            return property.GetValue(target);

        if (@this is FieldInfo field)
            return field.GetValue(target);

        if (@this is MethodInfo method)
            return method.Invoke(target, Array.Empty<object>());

        throw new Exception("GetValue() is not implemented for " + @this?.GetType().Name);
    }

    public static bool IsA<T>(this Type @this) => typeof(T).IsAssignableFrom(@this);

    public static bool IsA(this Type @this, Type type) => type.IsAssignableFrom(@this);

    public static T CreateInstance<T>(this Type @this, params object[] constructorParameters) =>
        (T) Activator.CreateInstance(@this, constructorParameters)!;

}