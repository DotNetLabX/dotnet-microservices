using System;
using System.ComponentModel;
using System.Reflection;

namespace Blocks.Core;

public static class EnumExtensions
{
		//public static string ToString2(this Enum value) => nameof(value);
        

		public static string ToDescription(this Enum value)
    {
				FieldInfo field = value.GetType().GetField(value.ToString());
        DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
        return attribute == null ? value.ToString() : attribute.Description;
    }

		public static IEnumerable<TEnum> GetValues<TEnum>()
        where TEnum : struct, Enum
    {
				return Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
		}
}
