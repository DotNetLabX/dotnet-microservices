using ArticleHub.Domain.Entities;
using Articles.Abstractions;
using Articles.IntegrationEvents.Contracts.Articles.Dtos;
using Mapster;

namespace ArticleHub.API.Articles.Consumers;

public class IntegrationEventsMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
				config.NewConfig<ArticleDto, Article>()
						.Ignore(dest => dest.Actors)
						.Ignore(dest => dest.SubmittedBy)
						.Ignore(dest => dest.Journal);
				//config.NewConfig<ArticleContributor, ContributorDto>();

    //    config.NewConfig<Person, PersonDto>();
    }
}
