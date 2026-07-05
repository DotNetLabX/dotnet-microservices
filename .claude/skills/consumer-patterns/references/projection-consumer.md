# Template: read-model projection consumer (ArticleHub)

A projection consumer writes to a **plain data-bag** entity — public get/set, `List<T>` collections, extends `Entity`, zero behavior (consumers-5). No aggregate factory, no domain invariants; you set properties or `Adapt` the DTO directly. ArticleHub registers no gRPC clients — every field it needs must already be in the event payload (datasharing-4).

The projection entity it writes to:

```csharp
public class Article : Entity
{
    public required string Title { get; set; }
    public string? Doi { get; set; }
    public ArticleStage Stage { get; set; }

    public required virtual int SubmittedById { get; set; }
    public virtual Person SubmittedBy { get; set; } = null!;

    public DateTime SubmittedOn { get; set; }
    public DateTime? AcceptedOn { get; set; }
    public DateTime? PublishedOn { get; set; }

    public required int JournalId { get; set; }
    public Journal Journal { get; set; } = null!;

    public List<ArticleActor> Actors { get; set; } = new();
}
```

## Variant A — create-new (first event in the lifecycle)

From `ArticleHub.API/Articles/Consumers/ArticleApprovedForReviewConsumer.cs`. Idempotency here is **throw-if-exists** via `AnyAsync` — the first stage should create the row exactly once (consumers-2).

```csharp
public class ArticleApprovedForReviewConsumer(ArticleHubDbContext _dbContext)
    : IConsumer<ArticleApprovedForReviewEvent>
{
    public async Task Consume(ConsumeContext<ArticleApprovedForReviewEvent> context)
    {
        var articleDto = context.Message.Article;

        // PHASE 2 — throw-if-exists.
        if (await _dbContext.Articles.AnyAsync(a => a.Id == articleDto.Id, context.CancellationToken))
            throw new BadRequestException("Article was already approved for review.");

        // PHASE 3 — GetOrCreate foreign reference data (local-first, Adapt if missing).
        var journal = await GetOrCreateJournalAsync(articleDto, context.CancellationToken);

        // PHASE 4 — projection write: no factory, just Adapt the DTO and set the foreign keys.
        var article = articleDto.AdaptWith<Article>(article =>
        {
            article.Journal = journal;
            article.SubmittedById = articleDto.SubmittedBy.Id;
        });

        await CreateActorsAsync(articleDto, article, context.CancellationToken);
        await _dbContext.Articles.AddAsync(article);

        // PHASE 5 — save.
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }

    private async Task<Journal> GetOrCreateJournalAsync(ArticleDto articleDto, CancellationToken ct = default)
    {
        var journal = await _dbContext.Journals.SingleOrDefaultAsync(j => j.Id == articleDto.Journal.Id, ct);
        if (journal is null)
        {
            journal = articleDto.Journal.Adapt<Journal>();
            await _dbContext.Journals.AddAsync(journal);
        }

        return journal;
    }

    private async Task CreateActorsAsync(ArticleDto articleDto, Article article, CancellationToken ct = default)
    {
        foreach (var actorDto in articleDto.Actors)
        {
            var person = await _dbContext.Persons.SingleOrDefaultAsync(p => p.Id == actorDto.Person.Id, ct);
            if (person is null)
            {
                person = actorDto.Person.Adapt<Person>();
                _dbContext.Persons.Add(person);
            }

            article.Actors.Add(new ArticleActor
            {
                ArticleId = article.Id,
                PersonId = person.Id,
                Role = actorDto.Role,
            });
        }
    }
}
```

## Variant B — update-existing (a later stage mutates the same row)

From `ArticleHub.API/Articles/Consumers/ArticleAcceptedForProductionConsumer.cs`. The row **must already exist** from an earlier stage, so idempotency inverts: load-or-throw with `SingleOrThrowAsync`, then mutate only the fields the stage can change.

```csharp
public sealed class ArticleAcceptedForProductionConsumer(ArticleHubDbContext _dbContext)
    : IConsumer<ArticleAcceptedForProductionEvent>
{
    public async Task Consume(ConsumeContext<ArticleAcceptedForProductionEvent> ctx)
    {
        var articleDto = ctx.Message.Article;

        // Must already exist from ApprovedForReview.
        var article = await _dbContext.Articles
            .Include(a => a.Actors)
            .SingleOrThrowAsync(a => a.Id == articleDto.Id);

        // Update only fields that can change during Review.
        article.Title = articleDto.Title;
        article.Stage = articleDto.Stage;

        await AddReviewers(articleDto, article);

        await _dbContext.SaveChangesAsync();
    }

    private async Task AddReviewers(ArticleDto articleDto, Article article)
    {
        foreach (var actorDto in articleDto.Actors.Where(a => a.Role == UserRoleType.REV))
        {
            var person = await _dbContext.Persons.FirstOrDefaultAsync(p => p.Id == actorDto.Person.Id);
            if (person is null)
            {
                person = actorDto.Person.Adapt<Person>();
                _dbContext.Persons.Add(person);
            }

            article.Actors.Add(new ArticleActor { Person = person, Role = actorDto.Role });
        }
    }
}
```

`SingleOrThrowAsync` is a `Blocks.EntityFrameworkCore` `IQueryable` extension that throws `NotFoundException` when the row is absent.
