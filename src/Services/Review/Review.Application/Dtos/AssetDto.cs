using Review.Domain.Assets.Enums;

namespace Review.Application.Dtos;

public record AssetDto(
		int Id, 
		string Name, 
		int Number, 
		AssetState State,
		AssetType Type,
		FileDto File);