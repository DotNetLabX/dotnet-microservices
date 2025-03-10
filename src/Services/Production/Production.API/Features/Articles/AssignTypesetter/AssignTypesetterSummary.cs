using Articles.Abstractions;
using FastEndpoints;

namespace Production.API.Features.Articles.AssignTypesetter;

public class AssignTypesetterSummary: Summary<AssignTypesetterEndpoint>
{
		public AssignTypesetterSummary()
		{
				//todo improve the summary
				Summary = "Updates an existing customer in the system";
				Description = "Updates an existing customer in the system";
				Response<IdResponse>(201, "Customer was successfully updated");
		}
}
