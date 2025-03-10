using FileStorage.Contracts;

namespace Submission.Domain.Entities;

public partial class File
{
		private File() {/* use factory method*/}

		internal static File CreateFile(FileMetadata fileMetadata, Asset asset, AssetTypeDefinition assetType)
		{
				var fileName = Path.GetFileName(fileMetadata.StoragePath);
				var extension = FileExtension.FromFileName(fileName, assetType);

				var file = new File()
				{
						Name = FileName.From(asset, extension),
						Extension = extension,
						OriginalName = fileName,
						Size = fileMetadata.FileSize,
						FileServerId = fileMetadata.FileId
				};
				return file;
		}
}