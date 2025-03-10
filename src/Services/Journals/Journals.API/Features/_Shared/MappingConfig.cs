using Mapster;
using Blocks.Mapster;
using Journals.Domain.Journals;

namespace Journals.API.Features.Shared;

public class MappingConfig : IRegister
{
		public void Register(TypeAdapterConfig config)
		{
				config.NewConfig<Journal, JournalDto>().MapToConstructor();
				config.NewConfig<Section, SectionDto>().MapToConstructor();
				config.NewConfig<Editor, EditorDto>().MapToConstructor();
		}
}
