using Articles.Abstractions;
using Auth.Domain.Persons.ValueObjects;
using Auth.Domain.Users;

namespace Auth.Domain.Persons;

public partial class Person
{
		public static Person Create(IPersonCreationInfo userInfo)
		{
				var person = new Person
				{
						Email = userInfo.Email,
						FirstName = userInfo.FirstName,
						LastName = userInfo.LastName,
						Gender = userInfo.Gender,
						PictureUrl = userInfo.PictureUrl,
						Honorific = HonorificTitle.FromEnum(userInfo.Honorific),
						ProfessionalProfile = ProfessionalProfile.Create(userInfo.Position, userInfo.CompanyName, userInfo.Affiliation),
						CreatedOn = DateTime.UtcNow
				};

				return person;
		}

		public void AssignUser(User user)
		{
				this.UserId = user.Id;
				this.Email.NormalizedEmail = user.NormalizedEmail!;
		}
}
