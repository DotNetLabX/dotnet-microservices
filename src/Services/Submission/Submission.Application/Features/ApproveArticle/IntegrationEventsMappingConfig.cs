using Articles.IntegrationEvents.Contracts.Articles.Dtos;

namespace Submission.Application.Features.ApproveArticle;

public class IntegrationEventsMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ArticleActor, ActorDto>()
                .Include<ArticleAuthor, ActorDto>();

        config.NewConfig<Person, PersonDto>()
                .Include<Author, PersonDto>();

        config.NewConfig<IArticleAction<ArticleActionType>, ArticleAction>()
                .Map(dest => dest.TypeId, src => src.ActionType);
    }
}
