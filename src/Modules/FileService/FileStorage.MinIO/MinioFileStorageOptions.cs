namespace FileStorage.MinIO;

public class MinioFileStorageOptions
{
		public string BucketName { get; init; } = "files";

		public long FileSizeLimitInMB { get; init; } = 50;
		public long FileSizeLimitInBytes => FileSizeLimitInMB * 1024 * 1024;
}
