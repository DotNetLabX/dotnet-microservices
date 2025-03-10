namespace Review.Application.Dtos;

public record FileDto(
		string Name,
		string OriginalName,
		string Extension,
		long Size,
		string FileServerId);