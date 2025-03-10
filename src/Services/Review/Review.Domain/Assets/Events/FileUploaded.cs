namespace Review.Domain.Assets.Events;

public record FileUploaded(Asset asset, IArticleAction action)
		: DomainEvent(action);
