using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace dotnet_demo_s3;

public class S3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3Service(IAmazonS3 s3Client, string bucketName)
    {
        _s3Client = s3Client;
        _bucketName = bucketName;
    }

    // Create - Upload an object to S3
    public async Task UploadObjectAsync(string filePath, string objectKey)
    {
        try
        {
            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(filePath, _bucketName, objectKey);
            Console.WriteLine($"Successfully uploaded {objectKey} to {_bucketName}");
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error uploading object: {ex.Message}");
        }
    }

    // Create - Upload string content as an object to S3
    public async Task UploadStringAsync(string content, string objectKey)
    {
        try
        {
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                ContentBody = content
            };

            await _s3Client.PutObjectAsync(putObjectRequest);
            Console.WriteLine($"Successfully uploaded {objectKey} to {_bucketName}");
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error uploading string as object: {ex.Message}");
        }
    }

    // Read - Download an object from S3
    public async Task<string> DownloadObjectAsStringAsync(string objectKey)
    {
        try
        {
            var getObjectRequest = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey
            };

            using var response = await _s3Client.GetObjectAsync(getObjectRequest);
            using var reader = new StreamReader(response.ResponseStream);
            var content = await reader.ReadToEndAsync();
            Console.WriteLine($"Successfully downloaded {objectKey}");
            return content;
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error downloading object: {ex.Message}");
            return string.Empty;
        }
    }

    // Update - Same as upload, S3 will overwrite existing object
    public async Task UpdateObjectAsync(string content, string objectKey)
    {
        await UploadStringAsync(content, objectKey);
        Console.WriteLine($"Successfully updated {objectKey}");
    }

    // Delete - Remove an object from S3
    public async Task DeleteObjectAsync(string objectKey)
    {
        try
        {
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey
            };

            await _s3Client.DeleteObjectAsync(deleteObjectRequest);
            Console.WriteLine($"Successfully deleted {objectKey} from {_bucketName}");
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error deleting object: {ex.Message}");
        }
    }

    // List - Get a list of objects from S3 bucket
    public async Task<List<S3Object>> ListObjectsAsync(string prefix = "")
    {
        try
        {
            var listObjectsRequest = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = prefix
            };

            var response = await _s3Client.ListObjectsV2Async(listObjectsRequest);
            Console.WriteLine($"Found {response.S3Objects.Count} objects in {_bucketName}");
            return response.S3Objects;
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error listing objects: {ex.Message}");
            return new List<S3Object>();
        }
    }
}
