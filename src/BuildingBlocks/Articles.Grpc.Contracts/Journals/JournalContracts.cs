using ProtoBuf;
using ProtoBuf.Grpc;
using System.ServiceModel;

namespace Journals.Grpc;

[ServiceContract]
public interface IJournalService
{
		[OperationContract]
		ValueTask<GetJournalResponse> GetJournalByIdAsync(GetJournalByIdRequest request, CallContext context = default);

		[OperationContract]
		ValueTask<IsEditorAssignedToJournalResponse> IsEditorAssignedToJournalAsync(IsEditorAssignedToJournalRequest request, CallContext context = default);
}

[ProtoContract]
public class GetJournalByIdRequest
{
		[ProtoMember(1)]
		public int JournalId { get; set; } = default!;
}

[ProtoContract]
public class GetJournalResponse
{
		[ProtoMember(1)]
		public JournalInfo Journal { get; set; } = default!;
}

[ProtoContract]
public class JournalInfo
{
		[ProtoMember(1)]
		public int Id { get; set; } = default!;

		[ProtoMember(2)]
		public string Name { get; set; } = default!;

		[ProtoMember(3)]
		public string Abbreviation { get; set; } = default!;

		[ProtoMember(4)]
		public int ChiefEditorUserId { get; set; } = default!;
}

[ProtoContract]
public class IsEditorAssignedToJournalRequest
{
		[ProtoMember(1)]
		public int JournalId { get; set; } = default!;

		[ProtoMember(2)]
		public int UserId { get; set; } = default!;
}

[ProtoContract]
public class IsEditorAssignedToJournalResponse
{
		[ProtoMember(1)]
		public bool IsAssigned { get; set; }
}
