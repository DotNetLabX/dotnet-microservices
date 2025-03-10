namespace Submission.API.Endpoints;

public static class EndpointRegistration
{
		public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app)
		{
				var api = app.MapGroup("/api");

				GetArticleEndpoint.Map(api);
				CreateArticleEndpoint.Map(api);
				AssignAuthorEndpoint.Map(api);
				CreateAndAssignAuthorEndpoint.Map(api);
				UploadManuscriptFileEndpoint.Map(api);
				UploadSupplementaryMaterialFileEndpoint.Map(api);
				SubmitArticleEndpoint.Map(api);
				ApproveArticleEndpoint.Map(api);
				RejectArticleEndpoint.Map(api);
				DownloadFileEndpoint.Map(api);
				
				return app;
		}
}
