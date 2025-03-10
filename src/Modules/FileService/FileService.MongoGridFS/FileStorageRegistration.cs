using Blocks.Core;
using FileStorage.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace FileStorage.MongoGridFS;

public static class FileStorageRegistration
{
		public static IServiceCollection AddMongoFileStorageAsSingletone(this IServiceCollection services, IConfiguration config)
				=> services.AddMongoFileStorageAsSingletone<FileService, MongoGridFsFileStorageOptions>(config); // default registration

		public static IServiceCollection AddMongoFileStorageAsSingletone<TService, TOptions>(this IServiceCollection services, IConfiguration config)
				where TService : FileService // just in case we might want to extend the FileService
				where TOptions : MongoGridFsFileStorageOptions
		{
				services.AddAndValidateOptions<TOptions>(config);
				var options = config.GetSectionByTypeName<TOptions>();

				services.AddSingleton<IMongoClient>(sp =>
				{
						return new MongoClient(config.GetConnectionStringOrThrow(options.ConnectionStringName));
				});

				services.AddSingleton(sp =>
				{
						var client = sp.GetRequiredService<IMongoClient>();
						return client.GetDatabase(options.DatabaseName);
				});

				services.AddSingleton(sp =>
				{
						var db = sp.GetRequiredService<IMongoDatabase>();
						return new GridFSBucket(db, new GridFSBucketOptions
						{
								BucketName = options.BucketName,
								ChunkSizeBytes = options.ChunkSizeBytes,
								WriteConcern = WriteConcern.WMajority,
								ReadPreference = ReadPreference.Primary
						});
				});

				services.AddScoped<IFileService, TService>();
				services.AddScoped<TService>(); 

				return services;
		}


		public static IServiceCollection AddMongoFileStorageAsScoped<TOptions>(this IServiceCollection services, IConfiguration config)
				where TOptions : MongoGridFsFileStorageOptions
		{
				services.AddAndValidateOptions<TOptions>(config);

				// TOptions is mandatory here so the DI will be able to register multiple IFileService
				services.AddScoped<IFileService<TOptions>>(sp =>
				{
						var options = sp.GetRequiredService<IOptions<TOptions>>();
						var optValue = options.Value;
						var client = new MongoClient(config.GetConnectionStringOrThrow(optValue.ConnectionStringName));
						var db = client.GetDatabase(optValue.DatabaseName);
						var bucket = new GridFSBucket(db, new GridFSBucketOptions
						{
								BucketName = optValue.BucketName,
								ChunkSizeBytes = optValue.ChunkSizeBytes,
								WriteConcern = WriteConcern.WMajority,
								ReadPreference = ReadPreference.Primary
						});

						return new FileService<TOptions>(bucket, options);
				});

				return services;
		}
}
