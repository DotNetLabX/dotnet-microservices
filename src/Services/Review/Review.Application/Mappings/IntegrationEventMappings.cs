using Articles.IntegrationEvents.Contracts.Articles.Dtos;
using Review.Domain.Shared;
using Review.Domain.Shared.Enums;

namespace Review.Application.Mappings;

public class IntegrationEventMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ArticleActor, ActorDto>()
                .Include<ArticleAuthor, ActorDto>();

        config.NewConfig<Person, PersonDto>()
                .Include<Author, PersonDto>();

        config.NewConfig<IArticleAction<ArticleActionType>, ArticleAction>();                
    }
}
