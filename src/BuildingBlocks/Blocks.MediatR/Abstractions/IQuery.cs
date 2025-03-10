using MediatR;

namespace Blocks.MediatR;

public interface IQuery<out TResponse> : IRequest<TResponse>
    where TResponse : notnull
{
}