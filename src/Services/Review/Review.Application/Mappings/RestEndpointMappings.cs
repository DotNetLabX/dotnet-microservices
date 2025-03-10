using Review.Application.Dtos;
using Review.Domain.Shared;
using Review.Domain.Shared.Enums;

namespace Review.Application.Mappings;

public class RestEndpointMappings : IRegister
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
