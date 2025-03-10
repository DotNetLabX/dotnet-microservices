using Blocks.Core;
using Auth.Domain.Persons;

namespace Auth.Domain.Users;
public partial class User
{
		public static User Create(IUserCreationInfo userInfo, Person person)
		{
				if (userInfo.UserRoles.IsNullOrEmpty())
						throw new ArgumentException("User must have at least one role.", nameof(userInfo.UserRoles));
				
				var user = new User
				{
						UserName = userInfo.Email,
						Email = userInfo.Email,
						PhoneNumber = userInfo.PhoneNumber,
						_userRoles = userInfo.UserRoles.Select(r => UserRole.Create(r)).ToList(),
						CreatedOn = DateTime.UtcNow,
						Person = person
				};

				///user.AddDomainEvent(new UserCreated(this));

				return user;
		}

		public void AssignRefreshToken(RefreshToken refreshToken)
		{
				if (refreshToken is null )
						throw new ArgumentNullException(nameof(refreshToken));
				_refreshTokens.Add(refreshToken);
		}
}
