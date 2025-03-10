using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Blocks.Core;

public abstract class Enumeration : IComparable
{
    private static ConcurrentDictionary<Type, IEnumerable<Enumeration>> _cache = new ConcurrentDictionary<Type, IEnumerable<Enumeration>>();

    public int Id { get; private set; }

    public string Name { get; private set; }

    public string Value { get; private set; }

    public string DisplayName => Name;

    [NotMapped]
    public string PropertyName { get; private set; }

    protected Enumeration(int id, string name, string value = null)
    {
        Id = id;
        Name = name;
        Value = value;
    }

    protected Enumeration()
    {
    }

    public override string ToString() => Name;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        return _cache.GetOrAdd(typeof(T), enumType => GetAllImpl<T>(enumType)).Cast<T>();
    }

    public static IEnumerable<Enumeration> GetAll(Type type)
    {
        return _cache.GetOrAdd(type, enumType => GetAllImpl<Enumeration>(enumType));
    }

    private static IEnumerable<T> GetAllImpl<T>(Type type) where T : Enumeration
    {
        var fields = Assembly
            .GetAssembly(type)
            .GetTypes()
            .Where(t => t.IsSubclassOf(type) || t == type)
            .SelectMany(t => t.GetPropertiesAndFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

        return fields.Select(f =>{
            var value = (T)f.GetValue(null);
            value.PropertyName = f.Name;
            return value;
        });
    }

    public override bool Equals(object obj)
    {
        var otherValue = obj as Enumeration;

        if (otherValue == null)
            return false;

        var typeMatches = GetType().Equals(obj.GetType());
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static int AbsoluteDifference(Enumeration firstValue, Enumeration secondValue)
    {
        return Math.Abs(firstValue.Id - secondValue.Id);
    }

    public static T FromValue<T>(string value, T defaultValue = null) where T : Enumeration
    {
        var matchingItem = Parse(value, "value", item => item.Value == value, defaultValue);
        return matchingItem;
    }

    public static T FromId<T>(int id) where T : Enumeration
    {
        var matchingItem = Parse<T, int>(id, "value", item => item.Id == id);
        return matchingItem;
    }

    public static T FromId<T>(int id, T valueForInvalidId = null) where T : Enumeration
    {
        var matchingItem = Parse(id, "value", item => item.Id == id, valueForInvalidId);
        return matchingItem;
    }

    public static T FromId<T>(int? id, T valueForInvalidId = null) where T : Enumeration
    {
        return id.HasValue ? FromId(id.Value, valueForInvalidId) : valueForInvalidId;
    }

    public static T FromDisplayName<T>(string displayName) where T : Enumeration
    {
        var matchingItem = Parse<T, string>(displayName, "display name", item => string.Equals(item.Name, displayName, StringComparison.InvariantCultureIgnoreCase));
        return matchingItem;
    }        

    private static T Parse<T, K>(K value, string description, Func<T, bool> predicate, T defaultValue = null) where T : Enumeration
    {
        var matchingItem = GetAll<T>().FirstOrDefault(predicate);

        if (matchingItem == null)
        {
            if (defaultValue != null)
                return defaultValue;
            else
                throw new InvalidOperationException($"'{value}' is not a valid {description} in {typeof(T)}");
        }

        return matchingItem;
    }

    public int CompareTo(object other) => Id.CompareTo(((Enumeration)other).Id);

    public static IEnumerable<T> GetAsCollection<T, R>(IEnumerable<R> types)
        where T : Enumeration
        where R : struct
    {
        foreach (var type in types)
            yield return FromId<T>(Convert.ToInt32(type));
    }
}