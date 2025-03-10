using Microsoft.Extensions.DependencyInjection;

namespace ArticleTimeline.Application.VariableResolvers;

public delegate IVariableResolver VariableResolverFactory(VariableResolverType resolverType);

public static class VariableResolverRegistration
{
    public static IServiceCollection AddArticleTimelineVariableResolvers(this IServiceCollection services)
    {
        services.AddScoped<CurrentUserRoleResolver>();
				services.AddScoped<CurrentUserNameResolver>();
				services.AddScoped<UploadedFileResolver>();
        services.AddScoped<ArticleCurrentStageResolver>();
				services.AddScoped<ArticleNewStageResolver>();
        services.AddScoped<MessageResolver>();

        services.AddScoped<VariableResolverFactory>(serviceProvider => variableType =>
        {
            return variableType switch
            {
                VariableResolverType.RoleUser => serviceProvider.GetService<CurrentUserRoleResolver>(),
                VariableResolverType.UploadedFile => serviceProvider.GetService<UploadedFileResolver>(),
								VariableResolverType.CurrentStage => serviceProvider.GetService<ArticleCurrentStageResolver>(),
								VariableResolverType.NewStage => serviceProvider.GetService<ArticleNewStageResolver>(),
                VariableResolverType.UserName => serviceProvider.GetService<CurrentUserNameResolver>(),
                VariableResolverType.Message => serviceProvider.GetService<MessageResolver>(),

                _ => throw new ApplicationException(),
            };
        });
        return services;
    }

}
