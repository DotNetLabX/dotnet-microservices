using Articles.Abstractions;
using Articles.Abstractions.Events;
using ArticleTimeline.Application.VariableResolvers;
using ArticleTimeline.Domain.Enums;
using ArticleTimeline.Persistence;
using ArticleTimeline.Persistence.Repositories;
using Blocks.EntityFrameworkCore;

namespace ArticleTimeline.Application.EventHandlers;

public class AddTimelineWhenArticleStageChangedEventHandler(
    TransactionProvider transactionProvider,
    TimelineRepository timelineRepository,
    ArticleTimelineDbContext dbContext,
    VariableResolverFactory variableResolverFactory)
    : AddTimelineEventHandler<ArticleStageChanged, IArticleAction>(
        transactionProvider, timelineRepository, dbContext, variableResolverFactory)
{
    protected override SourceType GetSourceType() => SourceType.StageTransition;
    protected override string GetSourceId(ArticleStageChanged eventModel) => $"{eventModel.CurrentStage}->{eventModel.NewStage}";
}
