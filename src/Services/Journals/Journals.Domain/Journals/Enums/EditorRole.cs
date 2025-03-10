using System.Text.Json.Serialization;

namespace Journals.Domain.Journals.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EditorRole
{
		ChiefEditor,
		ReviewEditor,
}
