using ProtoBuf;

namespace Articles.Abstractions.Enums;

[ProtoContract]
public enum Gender
{
		[ProtoEnum]
		NotDeclared = 0,
		[ProtoEnum]
		Male = 1,
		[ProtoEnum]
		Female = 2,
		[ProtoEnum]
		Neutral = 3
}
