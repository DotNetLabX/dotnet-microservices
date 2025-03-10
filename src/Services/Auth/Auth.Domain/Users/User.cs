using Microsoft.AspNetCore.Identity;
using Blocks.Domain;
using Auth.Domain.Persons;

namespace Auth.Domain.Users;

public partial class User : IdentityUser<int>, IAggregateRoot
{
		public DateTime RegistrationDate { get; init; } = DateTime.UtcNow;
		public DateTime? LastLogin { get; set; }

		public string FullName => Person.FullName;
		public int PersonId { get; set; }
		public Person Person { get; init; } = null!;

		private List<UserRole> _userRoles = new List<UserRole>();
		public virtual IReadOnlyList<UserRole> UserRoles => _userRoles;

		private List<RefreshToken> _refreshTokens = new();
		public virtual IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens;

		//Audit
		public int CreatedById { get; init; } = default!;
		public DateTime CreatedOn { get; init; } = DateTime.UtcNow;
		public int? LastModifiedById { get; set; } = 0;
		public DateTime? LastModifiedOn { get; set; } = DateTime.UtcNow;

		//Aggregate
		private List<IDomainEvent> _domainEvents = new();
		public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
		public void AddDomainEvent(IDomainEvent eventItem) => _domainEvents.Add(eventItem);
		public void ClearDomainEvents() => _domainEvents.Clear();
}
