using Production.Domain.Assets.Enums;

namespace Production.Application.Dtos;

public record AssetMinimalDto(int Id, AssetState State, FileMinimalDto? File);
