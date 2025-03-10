using Submission.Application.Dtos;

namespace Submission.Application.Mappings;

public class RestEndpointMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ArticleActor, ActorDto>()
                .Include<ArticleAuthor, ActorDto>();

        config.NewConfig<Person, PersonDto>()
                .Include<Author, PersonDto>();

        config.NewConfig<IArticleAction<ArticleActionType>, ArticleAction>()
                .Map(dest => dest.TypeId, src => src.ActionType);

				config.ForType<string, EmailAddress>()
						.MapWith(src => EmailAddress.Create(src));
		}
}
