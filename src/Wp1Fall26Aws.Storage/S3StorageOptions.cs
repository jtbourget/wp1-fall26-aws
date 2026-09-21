namespace Wp1Fall26Aws.Storage;

public class S3StorageOptions
{
    public string BucketName { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = "documents/uploads";
    public string Region { get; set; } = "us-east-1";
    public long MaxFileSizeBytes { get; set; } = 1048576;
}
