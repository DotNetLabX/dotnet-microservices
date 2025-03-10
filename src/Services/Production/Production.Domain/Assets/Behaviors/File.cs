using FileStorage.Contracts;
using Production.Domain.Assets.ValueObjects;

namespace Production.Domain.Assets;

public partial class File
{
    private File(){/* use factory method*/}

    public static File CreateFile(FileMetadata fileMetadata, Asset asset, AssetTypeDefinition assetTypeDefinition)
		{
				var fileName = Path.GetFileName(fileMetadata.StoragePath);
				var extension = FileExtension.FromFileName(fileName, assetTypeDefinition);

				var file = new File()
				{
						Name = FileName.From(asset, extension),
						Version = FileVersion.FromAsset(asset),
						Extension = extension,
						OriginalName = fileName,
						Size = fileMetadata.FileSize,
						FileServerId = fileMetadata.StoragePath
				};
				return file;
		}
}
