namespace Wp1Fall26Aws.Storage;

/// <summary>
/// Configuration options bound to the <c>"S3Storage"</c> section of application configuration.
/// </summary>
public class S3StorageOptions
{
    /// <summary>
    /// Gets or sets the target Amazon S3 bucket name.
    /// In local development, leaving this empty enables safe local mock mode.
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the root key prefix prepended to all generated S3 object keys.
    /// Defaults to <c>"documents/uploads"</c>.
    /// </summary>
    public string KeyPrefix { get; set; } = "documents/uploads";

    /// <summary>
    /// Gets or sets the AWS region for the bucket (e.g. <c>"us-east-1"</c>).
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// Gets or sets the maximum allowable file size in bytes for uploaded documents.
    /// Defaults to 1,048,576 bytes (1 MiB).
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 1048576;
}
