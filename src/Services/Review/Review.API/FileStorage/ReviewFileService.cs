using Microsoft.Extensions.Options;
using MongoDB.Driver.GridFS;
using FileStorage.MongoGridFS;

namespace Review.API.FileStorage;

//insight - named-subclass alternative to the generic FileService default the DI registers; deliberately not wired
public class ReviewFileService(GridFSBucket bucket, IOptions<ReviewFileStorageOptions> options)
    : FileService(bucket, options);
