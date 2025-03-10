using AutoMapper;
using Production.Application.Dtos;
using Production.Domain.Assets;

namespace Production.API.Features.Shared;

public class FileResponseMappingProfile : Profile
{
    public FileResponseMappingProfile()
    {
				CreateMap<Asset, AssetMinimalDto>()
						.IncludeMembers(src => src.CurrentFile);


				CreateMap<Domain.Assets.File, FileMinimalDto>();
    }
}
