using Amazon.S3;
using Amazon.S3.Model;

namespace Travel_Agency_Portal.Services;

public class S3StorageService
{
    private readonly IConfiguration _configuration;

    public S3StorageService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private IAmazonS3? CreateClient()
    {
        var serviceUrl = _configuration["Storage:S3:ServiceUrl"];
        var accessKey = _configuration["Storage:S3:AccessKey"];
        var secretKey = _configuration["Storage:S3:SecretKey"];

        if (string.IsNullOrWhiteSpace(serviceUrl) || string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
        {
            return null;
        }

        var config = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true
        };

        return new AmazonS3Client(accessKey, secretKey, config);
    }

    public async Task<bool> UploadTextAsync(string fileName, string content)
    {
        var client = CreateClient();
        if (client is null)
        {
            return false;
        }

        var bucket = _configuration["Storage:S3:Bucket"] ?? "travel-agency-files";
        try
        {
            await client.PutBucketAsync(new PutBucketRequest { BucketName = bucket });
        }
        catch
        {
            // Bucket may already exist.
        }

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        await client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = bucket,
            Key = fileName,
            InputStream = stream,
            ContentType = "text/plain"
        });

        return true;
    }
}
