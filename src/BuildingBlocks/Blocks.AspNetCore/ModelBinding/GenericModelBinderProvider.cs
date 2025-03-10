using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Blocks.AspNetCore.ModelBinding;

public class GenericModelBinderProvider : IModelBinderProvider
{
		public IModelBinder? GetBinder(ModelBinderProviderContext context)
		{
				var modelType = context.Metadata.ModelType;
				if (modelType.IsClass && modelType.Name.EndsWith("Command"))
				{
						var binderType = typeof(GenericModelBinder<>).MakeGenericType(modelType);
						return (IModelBinder)Activator.CreateInstance(binderType);
				}

				return null;
		}
}
