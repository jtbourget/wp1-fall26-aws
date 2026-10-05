using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Wp1Fall26Aws.Storage;
using Xunit;

namespace Wp1Fall26Aws.Tests;

public class S3DocumentStoreTests
{
    private readonly S3StorageOptions _options = new() { BucketName = "test-bucket", KeyPrefix = "documents/uploads", MaxFileSizeBytes = 1048576 };

    [Fact]
    public async Task KeyMatchesExpectedFormat()
    {
        var fake = new FakeS3ObjectClient();
        var store = new S3DocumentStore(fake, Options.Create(_options));
        var upload = new DocumentUpload("test.txt", "text/plain", 100, new MemoryStream(new byte[100]));
        var result = await store.StoreAsync(upload);

        Assert.True(result.Success);
        Assert.Matches(@"^documents/uploads/\d{4}/\d{2}/\d{2}/[0-9a-f]{32}\.txt$", result.ObjectKey);
    }

    [Fact]
    public async Task SameFilenameDifferentKeys()
    {
        var fake = new FakeS3ObjectClient();
        var store = new S3DocumentStore(fake, Options.Create(_options));
        var upload = new DocumentUpload("test.txt", "text/plain", 100, new MemoryStream(new byte[100]));
        var result1 = await store.StoreAsync(upload);
        var result2 = await store.StoreAsync(upload);

        Assert.NotEqual(result1.ObjectKey, result2.ObjectKey);
    }

    [Fact]
    public async Task PassesConfiguredBucketAndMetadata()
    {
        var fake = new FakeS3ObjectClient();
        var store = new S3DocumentStore(fake, Options.Create(_options));
        var upload = new DocumentUpload("test.txt", "text/plain", 100, new MemoryStream(new byte[100]));
        await store.StoreAsync(upload);

        var call = Assert.Single(fake.Calls);
        Assert.Equal("test-bucket", call.Bucket);
        Assert.Equal("text/plain", call.ContentType);
        Assert.Contains("original-filename", call.Metadata.Keys);
        Assert.Contains("content-type", call.Metadata.Keys);
        Assert.Contains("uploaded-utc", call.Metadata.Keys);
        Assert.Contains("content-length", call.Metadata.Keys);
    }

    [Fact]
    public async Task RejectedUploadCallsFakeZeroTimes()
    {
        var fake = new FakeS3ObjectClient();
        var store = new S3DocumentStore(fake, Options.Create(_options));
        var upload = new DocumentUpload("test.exe", "text/plain", 100, new MemoryStream(new byte[100]));
        var result = await store.StoreAsync(upload);

        Assert.False(result.Success);
        Assert.Empty(fake.Calls);
    }

    [Fact]
    public async Task StoreAsync_ValidDocument_ReturnsSuccessAndCallsS3()
    {
        // Arrange
        var spy = new RecordingS3ObjectClientSpy();
        var store = new S3DocumentStore(spy, Options.Create(_options));
        var upload = new DocumentUpload("test.txt", "text/plain", 18, new MemoryStream("synthetic lab file"u8.ToArray()));

        // Act
        var result = await store.StoreAsync(upload);

        // Assert
        Assert.False(result.Success); // Deliberately broken for demonstration
        Assert.NotNull(result.ObjectKey);
        
        Assert.NotNull(spy.LastRequest);
        Assert.Equal("test.txt", spy.LastRequest.Metadata["original-filename"]);
    }

    [Fact]
    public async Task StoreAsync_InvalidDocument_ReturnsFalseAndNeverCallsS3()
    {
        // Arrange
        var spy = new RecordingS3ObjectClientSpy();
        var store = new S3DocumentStore(spy, Options.Create(_options));
        var upload = new DocumentUpload("test.exe", "text/plain", 18, new MemoryStream("synthetic lab file"u8.ToArray()));

        // Act
        var result = await store.StoreAsync(upload);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.ObjectKey);
        Assert.Null(spy.LastRequest);
    }
}

