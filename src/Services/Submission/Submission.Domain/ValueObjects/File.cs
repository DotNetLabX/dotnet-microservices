namespace Submission.Domain.Entities;

// NOTE: This is a Value Object but kept in Entities namespace to avoid System.IO.File name ambiguity

public partial class File : IValueObject
{
    //insight - difference between required, null!, default!
    public required string OriginalName { get; init; } = default!;    
    //insight - server id, create the file server server both mongo & azure blob
    public required string FileServerId { get; init; } = default!;
    public required long Size { get; init; }
    //insight - differencce between required property and IsRequired Ef.Core configuration
    public required FileName Name { get; init; }
		public required FileExtension Extension { get; init; } = default!;
}