using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace Blocks.EntityFrameworkCore;

public static class BuilderExtensions
{
		public static PropertyBuilder<TEnum> HasEnumConversion<TEnum>(this PropertyBuilder<TEnum> builder, int maxLength = 64)
        where TEnum : Enum
    {
        return builder
				.HasConversion(
						e => e.ToString(),
						value => (TEnum)Enum.Parse(typeof(TEnum), value))
				.HasMaxLength(maxLength);
    }

		public static PropertyBuilder<TEnum> HasEnumAsStringConversion<TEnum>(this PropertyBuilder<TEnum> builder, int maxLength = 64)
				where TEnum : Enum
		{
				return builder.HasConversion<string>().HasMaxLength(maxLength);
		}

		public static PropertyBuilder<TEnum> HasEnumToStringConversion<TEnum>(this PropertyBuilder<TEnum> builder, int maxLength = 64)
				where TEnum : struct, Enum
		{
				return builder
						.HasConversion(new EnumToStringConverter<TEnum>())
						.HasMaxLength(maxLength);
		}

		public static PropertyBuilder<IReadOnlyList<string>> HasCsvListConversion(this PropertyBuilder<IReadOnlyList<string>> builder)
		{
				Func<IReadOnlyList<string>, string> serializeFunc = v => v != null && v.Any() ? string.Join(',', v) : string.Empty;
				Func<string, IReadOnlyList<string>> deserializeFunc = v =>
						!string.IsNullOrEmpty(v) ? v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();

				var csvListConverter = new ValueConverter<IReadOnlyList<string>, string>(
						v => serializeFunc(v),
						v => deserializeFunc(v)
				);

				return builder.HasConversion(csvListConverter);
		}

		public static ValueConverter<IReadOnlyList<T>, string> BuildJsonReadOnlyListConvertor<T>()
		{
				Func<IReadOnlyList<T>, string> serializeFunc = v => JsonSerializer.Serialize(v);
				Func<string, IReadOnlyList<T>> deserializeFunc = v => JsonSerializer.Deserialize<IReadOnlyList<T>>(v ?? "[]")!;

				return new ValueConverter<IReadOnlyList<T>, string>(
						v => serializeFunc(v),
						v => deserializeFunc(v));
		}

		public static ValueConverter<TCollection, string> BuildJsonListConvertor<TCollection>()
		{
				Func<TCollection, string> serializeFunc = v => JsonSerializer.Serialize(v);
				Func<string, TCollection> deserializeFunc = v => JsonSerializer.Deserialize<TCollection>(v ?? "[]")!;

				return new ValueConverter<TCollection, string>(
						v => serializeFunc(v),
						v => deserializeFunc(v));
		}

		public static PropertyBuilder<IReadOnlyList<T>> HasJsonReadOnlyListConversion<T>(this PropertyBuilder<IReadOnlyList<T>> builder)
				=> builder.HasConversion(BuildJsonReadOnlyListConvertor<T>());

		public static PropertyBuilder<T> HasJsonCollectionConversion<T>(this PropertyBuilder<T> builder)
				=> builder.HasConversion(BuildJsonListConvertor<T>());

		public static ComplexTypePropertyBuilder<T> HasJsonListConversion<T>(this ComplexTypePropertyBuilder<T> builder)
				=> builder.HasConversion(BuildJsonReadOnlyListConvertor<T>());

		public static PropertyBuilder<TCollection> HasJsonListConversion<TCollection, T>(this PropertyBuilder<TCollection> builder)
				where TCollection : IList<T>
				=> builder.HasConversion(BuildJsonListConvertor<TCollection>());

		public static PropertyBuilder<TProperty> HasColumnNameSameAsProperty<TProperty>(this PropertyBuilder<TProperty> builder)
				=> builder.HasColumnName(builder.Metadata.PropertyInfo?.Name);
		public static ComplexTypePropertyBuilder<TProperty> HasColumnNameSameAsProperty<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder)
				=> builder.HasColumnName(builder.Metadata.PropertyInfo?.Name);
}
