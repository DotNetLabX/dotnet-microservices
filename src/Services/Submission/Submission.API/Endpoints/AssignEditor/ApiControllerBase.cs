using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Production.API.Features.Shared;


[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase(IMediator _mediator) : ControllerBase
{
    //protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetService<ISender>()!;

    protected async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        return await _mediator.Send(request);
    }

    protected async Task<TResponse> HandleAsync<TResponse>(IRequest<TResponse> request)
    {
        return await _mediator.Send(request);
    }
}
