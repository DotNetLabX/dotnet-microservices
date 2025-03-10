namespace Production.Application.Dtos;

public record FileDto(
		int Id,
		string Name,
		string OriginalName,
		int Version,
		long Size,
		string FileServerId);