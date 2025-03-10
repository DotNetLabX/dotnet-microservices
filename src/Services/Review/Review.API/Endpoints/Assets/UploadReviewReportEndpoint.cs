using Microsoft.AspNetCore.Mvc;
using Review.Application.Features.Assets.UploadFiles.UploadReviewReport;

namespace Review.API.Endpoints.Assets
{
    public class UploadReviewReportEndpoint : ICarterModule
		{
				public void AddRoutes(IEndpointRouteBuilder app)
				{
						app.MapPost("/articles/{articleId:int}/assets/review-reports:upload",
								async ([FromRoute] int articleId, [FromForm] UploadReviewReportCommand command, ISender sender) =>
						{
								command.ArticleId = articleId;
								var response = await sender.Send(command);
								return Results.Created($"/api/articles/{command.ArticleId}/assets/{response.Id}:download", response);
						})
						.RequireRoleAuthorization(Role.Reviewer)
						.WithName("UploadReviewReport")
						.WithTags("Assets")
						.Produces<IdResponse>(StatusCodes.Status201Created)
						.ProducesProblem(StatusCodes.Status400BadRequest)
						.ProducesProblem(StatusCodes.Status404NotFound)
						.ProducesProblem(StatusCodes.Status401Unauthorized)
						.DisableAntiforgery(); // because of IFormFile
				}
		}
}
