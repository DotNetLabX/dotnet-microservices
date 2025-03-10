using FileStorage.Contracts;

namespace Submission.Application.Features.DownloadFile;

public class DownloadFileCommandHandler(AssetRepository _assetRepository, IFileService _fileService) 
		: IRequestHandler<DownloadFileQuery, DownloadFileResponse>
{
		public async Task<DownloadFileResponse> Handle(DownloadFileQuery command, CancellationToken ct)
		{
				var asset = Guard.NotFound(
						await _assetRepository.GetByIdAsync(command.ArticleId, command.AssetId)
						);

				var (fileStream, fileMetadata) = await _fileService.DownloadAsync(asset!.File.FileServerId, ct);

				return new DownloadFileResponse(asset.File.Name.Value, fileMetadata.ContentType, fileStream);
		}
}