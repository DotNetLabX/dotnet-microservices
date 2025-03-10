using Auth.Grpc;

namespace Submission.Domain.Entities;

public partial class Author
{      
		public static new Author Create(PersonInfo personInfo, IArticleAction action)
		{
				var author = new Author
				{
						Id = personInfo.Id,
						UserId = personInfo.UserId,
						Email = EmailAddress.Create(personInfo.Email),
						FirstName = personInfo.FirstName,
						LastName = personInfo.LastName,
						Honorific = personInfo.Honorific,
						Affiliation = personInfo.ProfessionalProfile!.Affiliation,
						CreatedById = action.CreatedById,
						CreatedOn = DateTime.UtcNow
				};

				var domainEvent = new AuthorCreated(author, action);
				author.AddDomainEvent(domainEvent);

				return author;
		}		
}
