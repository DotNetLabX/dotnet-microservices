using Review.Domain.Shared.ValueObjects;

namespace Review.Domain.Invitations.ValueObjects;

//public class Invitee
//{
//		public int? UserId { get; private set; }
//		public string? FirstName { get; private set; }
//		public string? LastName { get; private set; }
//		public EmailAddress? Email { get; private set; }

//		private Invitee() { } // EF

//		public static Invitee ForExistingUser(int userId) =>
//				new Invitee { UserId = userId, Email = email };

//		public static Invitee ForExternal(string firstName, string lastName, EmailAddress email) =>
//				new Invitee { FirstName = firstName, LastName = lastName, Email = email };

//		public bool IsExistingUser => UserId.HasValue;

//		public string FullName => IsExistingUser ? throw new InvalidOperationException() : $"{FirstName} {LastName}";
//}
