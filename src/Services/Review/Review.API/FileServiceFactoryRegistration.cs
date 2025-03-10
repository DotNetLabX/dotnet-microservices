using FileStorage.Contracts;
using FileStorage.MongoGridFS;
using Review.API.FileStorage;
using Review.Application.FileStorage;

namespace Review.API;

public static class FileServiceFactoryRegistration
{
		public static IServiceCollection AddFileServiceFactory(this IServiceCollection services)
		{
				services.AddScoped<FileServiceFactory>(serviceProvider => fileStorageType =>
				{
						return fileStorageType switch
						{
								FileStorageType.Submission => serviceProvider.GetRequiredService<IFileService<SubmissionFileStorageOptions>>(),
								FileStorageType.Review => serviceProvider.GetRequiredService<IFileService<MongoGridFsFileStorageOptions>>(),
								_ => throw new ApplicationException()
						};
				});
				return services;
		}
}
