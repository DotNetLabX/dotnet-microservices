using FileStorage.Contracts;
using System.ComponentModel.DataAnnotations;

namespace FileStorage.MongoGridFS;

public class MongoGridFsFileStorageOptions : MongoDbOptions
{
		public string BucketName { get; init; } = "files";
		public int ChunkSizeBytes { get; init; } = 1048576; // 1MB

		public long FileSizeLimitInMB { get; init; } = 50;
		public long FileSizeLimitInBytes => FileSizeLimitInMB * 1024 * 1024;
}

public class MongoDbOptions : IFileStorageOptions
{
		[Required]
		public string ConnectionStringName { get; init; } = default!;
		[Required]
		public string DatabaseName { get; init; } = default!;		
}