using Auth.Grpc;
using Review.Domain.Shared.ValueObjects;

namespace Review.Domain.Articles;

public partial class Author
{
		public static Author AcceptSubmitted(PersonInfo personInfo)
		{
				var person = new Author
				{
						Id = personInfo.Id,
						UserId = personInfo.UserId,
						Email = EmailAddress.Create(personInfo.Email),
						FirstName = personInfo.FirstName,
						LastName = personInfo.LastName,
						Honorific = personInfo.Honorific,
						Affiliation = personInfo.ProfessionalProfile?.Affiliation ?? string.Empty,						
				};

				return person;
		}
}
