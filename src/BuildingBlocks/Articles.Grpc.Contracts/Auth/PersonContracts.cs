using Articles.Abstractions;
using Articles.Abstractions.Enums;
using ProtoBuf;
using ProtoBuf.Grpc;
using System.ServiceModel;

namespace Auth.Grpc;

[ServiceContract]
public interface IPersonService
{
		[OperationContract]
		ValueTask<GetPersonResponse> GetPersonByIdAsync(GetPersonRequest request, CallContext context = default);
		[OperationContract]
		ValueTask<GetPersonResponse> GetPersonByUserIdAsync(GetPersonByUserIdRequest request, CallContext context = default);
		[OperationContract]
		ValueTask<GetPersonResponse> GetPersonByEmailAsync(GetPersonByEmailRequest request, CallContext context = default);
		[OperationContract]
		ValueTask<CreatePersonResponse> GetOrCreatePersonAsync(CreatePersonRequest request, CallContext context = default);
}

[ProtoContract]
public class ProfessionalProfile
{
		[ProtoMember(1)]
		public string Position { get; set; } = default!;

		[ProtoMember(2)]
		public string CompanyName { get; set; } = default!;

		[ProtoMember(3)]
		public string Affiliation { get; set; } = default!;
}

[ProtoContract]
public class PersonInfo
{
		[ProtoMember(1)]
		public int Id { get; set; }

		[ProtoMember(2)]
		public string FirstName { get; set; } = default!;

		[ProtoMember(3)]
		public string LastName { get; set; } = default!;

		[ProtoMember(4)]
		public string Email { get; set; } = default!;

		[ProtoMember(5)]
		public Gender Gender { get; set; }

		[ProtoMember(6, IsRequired = false)]
		public string? Honorific { get; set; }

		[ProtoMember(7, IsRequired = false)]
		public string? PictureUrl { get; set; }

		[ProtoMember(8, IsRequired = false)]
		public ProfessionalProfile? ProfessionalProfile { get; set; }

		[ProtoMember(9, IsRequired = false)]
		public int? UserId { get; set; }
}

[ProtoContract]
public class GetPersonRequest
{
		[ProtoMember(1)]
		public int PersonId { get; set; }
}

[ProtoContract]
public class GetPersonByUserIdRequest
{
		[ProtoMember(1)]
		public int UserId { get; set; }
}

[ProtoContract]
public class GetPersonByEmailRequest
{
		[ProtoMember(1)]
		public string Email { get; set; } = default!;
}

[ProtoContract]
public class GetPersonResponse
{
		[ProtoMember(1)]
		public PersonInfo PersonInfo { get; set; } = default!;
}

[ProtoContract]
public class CreatePersonRequest : IPersonCreationInfo
{
		[ProtoMember(1)]
		public string Email { get; set; } = default!;

		[ProtoMember(2)]
		public string FirstName { get; set; } = default!;

		[ProtoMember(3)]
		public string LastName { get; set; } = default!;

		[ProtoMember(4)]
		public Gender Gender { get; set; }

		[ProtoMember(5, IsRequired = false)]
		public Honorific? Honorific { get; set; }

		[ProtoMember(6, IsRequired = false)]
		public string? PictureUrl { get; set; }

		[ProtoMember(7, IsRequired = false)]
		public string? CompanyName { get; set; }

		[ProtoMember(8, IsRequired = false)]
		public string? Position { get; set; }

		[ProtoMember(9, IsRequired = false)]
		public string? Affiliation { get; set; }
}

[ProtoContract]
public class CreatePersonResponse
{
		[ProtoMember(1)]
		public PersonInfo PersonInfo { get; set; } = default!;
}
