using Articles.Security;
using Blocks.Domain;
using Blocks.Mapster;
using Blocks.MediatR.Behaviours;
using Blocks.Messaging.MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Review.Application.Features.Invitations.InviteReviewer;
using Review.Application.Mappings;
using Review.Application.StateMachines;
using System.Reflection;

namespace Review.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices (this IServiceCollection services, IConfiguration configuration)
    {
				services
						.AddMapsterConfigsFromAssemblyContaining<IntegrationEventMappings>()        // Register mapster configurations
						.AddValidatorsFromAssemblyContaining<InviteReviewerCommandValidator>()			// Register Fluent validators as transient
						.AddMediatR(config =>
						{
								config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

								config.AddOpenBehavior(typeof(AssignUserIdBehavior<,>));
								config.AddOpenBehavior(typeof(ValidationBehavior<,>));
								config.AddOpenBehavior(typeof(LoggingBehavior<,>));
						})
						.AddMassTransitWithRabbitMQ(configuration, Assembly.GetExecutingAssembly()); ;

				services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
				services.AddScoped<IArticleAccessChecker, ArticleAccessChecker>();

				services.AddScoped<ArticleStateMachineFactory>(provider => articleStage =>
				{
						var cache = provider.GetRequiredService<IMemoryCache>();
						return new ArticleStateMachine(articleStage, cache);
				});

				return services;
    }
}
