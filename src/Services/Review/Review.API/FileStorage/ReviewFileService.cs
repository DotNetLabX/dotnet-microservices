using Microsoft.Extensions.Options;
using MongoDB.Driver.GridFS;
using FileStorage.MongoGridFS;

namespace Review.API.FileStorage;

public class ReviewFileService(GridFSBucket bucket, IOptions<ReviewFileStorageOptions> options)
		: FileService(bucket, options);