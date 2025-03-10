using System;
using System.Text.Json.Serialization;

namespace Articles.Abstractions;

public abstract record ArticleCommandBase<TActionType>: IArticleAction<TActionType>
				where TActionType : Enum
{
		[JsonIgnore]
		public int ArticleId { get; set; }

		public string? Comment { get; init; }

		[JsonIgnore]
		public abstract TActionType ActionType { get; }

		[JsonIgnore]
		public string Action => ActionType.ToString();

		[JsonIgnore]
		public DateTime CreatedOn => DateTime.UtcNow;

		[JsonIgnore]
		public int CreatedById { get; set; }
}