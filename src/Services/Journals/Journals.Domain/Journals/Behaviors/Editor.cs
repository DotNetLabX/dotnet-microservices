using Auth.Grpc;

namespace Journals.Domain.Journals;

public partial class Editor
{
		public static Editor Create(PersonInfo personInfo)
		{
				return new Editor
				{
						Id = personInfo.UserId!.Value,
						PersonId = personInfo.Id,
						Email = personInfo.Email,
						Affiliation = personInfo.ProfessionalProfile!.Affiliation,
						FullName = personInfo.FirstName + " " + personInfo.LastName,
				};
		}
}
