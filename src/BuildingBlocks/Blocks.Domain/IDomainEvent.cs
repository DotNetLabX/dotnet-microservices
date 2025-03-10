using FastEndpoints;
using MediatR;

namespace Blocks.Domain;

public interface IDomainEvent : INotification, IEvent;
