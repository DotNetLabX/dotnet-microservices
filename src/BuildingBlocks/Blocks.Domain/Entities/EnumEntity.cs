namespace Blocks.Entitities;

public abstract class EnumEntity<TEnum> : Entity<TEnum>
		where TEnum : struct, Enum
{
		public TEnum Name { get; init; } = default!;
		public string Description { get; init; } = null!;
}