using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blocks.Core;

public static class JsonExtensions
{
		public static readonly JsonSerializerOptions DefaultOptions;

		static JsonExtensions()
    {
				DefaultOptions = new JsonSerializerOptions() 
				{ 
						PropertyNameCaseInsensitive = true,
						AllowTrailingCommas = true,
						IncludeFields = true,
						ReadCommentHandling = JsonCommentHandling.Skip
				};
				DefaultOptions.Converters.Add(new JsonStringEnumConverter());
		}

		public static T DeserializeCaseInsensitive<T>(string json)
		{
				return JsonSerializer.Deserialize<T>(json, DefaultOptions);
		}
}

