using FileStorage.Contracts;

namespace Review.Application.FileStorage;

public enum FileStorageType
{
		Review,
		Submission
}

public delegate IFileService FileServiceFactory(FileStorageType fileStorageType);
