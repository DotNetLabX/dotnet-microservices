using Articles.Abstractions;
using FastEndpoints;

namespace Production.API.Features.Articles.AssignTypesetter;

public class AssignTypesetterSummary : Summary<AssignTypesetterEndpoint>
{
    public AssignTypesetterSummary()
    {
        Summary = "Assigns a typesetter to an article";
        Description = "Assigns a typesetter to an article, transitioning it to the InProduction stage";
        Response<IdResponse>(200, "Typesetter was successfully assigned");
    }
}
