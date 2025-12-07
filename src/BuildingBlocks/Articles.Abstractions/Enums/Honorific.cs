using ProtoBuf;

namespace Articles.Abstractions.Enums;

[ProtoContract]
public enum Honorific
{
    [ProtoEnum]
    Mr,
    [ProtoEnum]
    Ms,
    [ProtoEnum]
    Dr,
    [ProtoEnum]
    Prof
}
