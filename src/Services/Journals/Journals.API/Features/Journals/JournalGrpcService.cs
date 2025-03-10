using Blocks.Redis;
using Journals.Grpc;
using ProtoBuf.Grpc;

namespace Journals.API.Features.Journals;

public class JournalGrpcService(Repository<Journal> _journalRepository) : IJournalService
{
		public async ValueTask<GetJournalResponse> GetJournalByIdAsync(GetJournalByIdRequest request, CallContext context = default)
		{
				//todo - Implement an interceptor which will tranform the exception into a valid GRPC error response
				var journal = await _journalRepository.GetByIdOrThrowAsync(request.JournalId); // talk - we don't send CT here because Redis.OM doesn't support them for the async methods
				return new GetJournalResponse
				{
						Journal = journal.Adapt<JournalInfo>()
				};
		}

		public async ValueTask<IsEditorAssignedToJournalResponse> IsEditorAssignedToJournalAsync(IsEditorAssignedToJournalRequest request, CallContext context = default)
		{
				var journal = await _journalRepository.GetByIdOrThrowAsync(request.JournalId);
				return new IsEditorAssignedToJournalResponse
				{
						IsAssigned = journal.ChiefEditorId == request.UserId
				};
		}
}
