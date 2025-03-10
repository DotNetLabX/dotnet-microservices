using Auth.Grpc;

namespace Auth.API.Mappings;

public class GrpcTypeAdapterConfig : TypeAdapterConfig
{
		public GrpcTypeAdapterConfig()
		{
				// this is for standard GRPC, we can safely remove it
				this.ForType<string?, string>()
						.MapWith(src => src ?? string.Empty);

				this.NewConfig<Person, PersonInfo>()
						.IgnoreNullValues(true); //mapster might have problems managing null values.
					

				this.NewConfig<Auth.Domain.Persons.ValueObjects.ProfessionalProfile, ProfessionalProfile>();
		}
}
