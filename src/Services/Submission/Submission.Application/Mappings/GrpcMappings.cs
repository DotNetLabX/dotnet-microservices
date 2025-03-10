using Auth.Grpc;
namespace Submission.Application.Mappings;

public class GrpcMappings : IRegister
{
		public void Register(TypeAdapterConfig config)
		{
				config.ForType<string, EmailAddress>()
						.MapWith(src => EmailAddress.Create(src));

				config.ForType<PersonInfo, Author>()
						.Map(dest => dest.UserId, src => src.Id)
						.Ignore(dest => dest.Id);
		}
}
