using Articles.Abstractions.Enums;
using Blocks.Core;
using Newtonsoft.Json;

namespace Auth.Domain.Persons.ValueObjects;

public class HonorificTitle : StringValueObject
{
    [JsonConstructor]
    private HonorificTitle(string value) => Value = value;

    public static HonorificTitle FromString(string honorific)
    {
        Guard.ThrowIfNullOrWhiteSpace(honorific);

        return new HonorificTitle(honorific.Trim());
    }

    public static HonorificTitle? FromEnum(Honorific? honorific)
    {
				if(honorific.HasValue)
				    return new HonorificTitle(honorific!.ToString());

        return null;
    }
}
