using Articles.Abstractions.Enums;

namespace Articles.IntegrationEvents.Contracts.Articles.Dtos;

public record class AssetDto(
		int Id,
		string Name,
		int Number,
		AssetType Type,
		FileDto File
		);

public record class FileDto(
		string OriginalName,
		string Name,
		string Extension,
		string FileServerId,
		long Size
		);
