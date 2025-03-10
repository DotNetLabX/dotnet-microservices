using FileStorage.Contracts;

namespace Submission.Domain.Entities;

public partial class Asset
{
		private Asset() {/* use factory method*/}

		public string GenerateStorageFilePath(string fileName) 
				=> $"Articles/{ArticleId}/{Name}/{fileName}";

		//insight - use internal factory method so that the Asset can be created only in the Domain
		internal static Asset Create(Article article, AssetTypeDefinition type, int assetCount, IArticleAction action)
		{
				//insight - value objects for AssetName & AssetNumber, encapsulate validation						
				return new Asset()
				{
						ArticleId = article.Id,
						Article = article,
						Name = AssetName.Create(type, assetCount),
						Number = AssetNumber.Create(type, assetCount),
						Type = type.Name,
						TypeRef = type,
						State = AssetState.None,
						CreatedById = action.CreatedById,
						CreatedOn = action.CreatedOn
				};
		}

		public File CreateFile(FileMetadata fileMetadata, AssetTypeDefinition assetType, IArticleAction<ArticleActionType> action)
		{
				File = File.CreateFile(fileMetadata, this, assetType);
				State = AssetState.Uploaded;
				return File;
		}
}
