using Articles.Abstractions;
using ArticleTimeline.Application.VariableResolvers;
using ArticleTimeline.Domain.Enums;
using ArticleTimeline.Persistence;
using ArticleTimeline.Persistence.Repositories;
using Blocks.EntityFrameworkCore;
using Production.Domain.Assets.Enums;
using Production.Domain.Assets.Events;

namespace ArticleTimeline.Application.EventHandlers;

public class AddTimelineWhenActionExecutedEventHandler(TransactionProvider transactionProvider, TimelineRepository timelineRepository, ArticleTimelineDbContext dbContext, VariableResolverFactory variableResolverFactory)
		: AddTimelineEventHandler<AssetActionExecuted, IArticleAction<AssetActionType>>(transactionProvider, timelineRepository, dbContext, variableResolverFactory)
{
		protected override SourceType GetSourceType() => SourceType.ActionExecuted;
		protected override string GetSourceId(AssetActionExecuted eventModel) => $"{eventModel.action.Action}";
}
