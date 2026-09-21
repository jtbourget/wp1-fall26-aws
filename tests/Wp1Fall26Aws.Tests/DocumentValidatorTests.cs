using System.IO;
using System.Linq;
using Wp1Fall26Aws.Storage;
using Xunit;

namespace Wp1Fall26Aws.Tests;

public class DocumentValidatorTests
{
    private readonly S3StorageOptions _options = new() { MaxFileSizeBytes = 1048576 };

    [Fact]
    public void AcceptsValidSmallTxt()
    {
        var upload = new DocumentUpload("test.txt", "text/plain", 100, new MemoryStream(new byte[100]));
        var errors = DocumentValidator.Validate(upload, _options);
        Assert.Empty(errors);
    }

    [Fact]
    public void RejectsEmpty()
    {
        var upload = new DocumentUpload("test.txt", "text/plain", 0, new MemoryStream());
        var errors = DocumentValidator.Validate(upload, _options);
        Assert.Contains(errors, e => e == "File is empty.");
    }

    [Fact]
    public void RejectsOversize_PassesAtBoundary()
    {
        var upload1 = new DocumentUpload("test.txt", "text/plain", 1048576, new MemoryStream());
        var errors1 = DocumentValidator.Validate(upload1, _options);
        Assert.DoesNotContain(errors1, e => e.Contains("exceeds the maximum size"));

        var upload2 = new DocumentUpload("test.txt", "text/plain", 1048577, new MemoryStream());
        var errors2 = DocumentValidator.Validate(upload2, _options);
        Assert.Contains(errors2, e => e.Contains("exceeds the maximum size"));
    }

    [Fact]
    public void RejectsExe()
    {
        var upload = new DocumentUpload("test.exe", "text/plain", 100, new MemoryStream());
        var errors = DocumentValidator.Validate(upload, _options);
        Assert.Contains(errors, e => e.Contains("not allowed"));
    }

    [Fact]
    public void RejectsDisallowedContentType()
    {
        var upload = new DocumentUpload("test.txt", "application/json", 100, new MemoryStream());
        var errors = DocumentValidator.Validate(upload, _options);
        Assert.Contains(errors, e => e.Contains("not allowed"));
    }

    [Fact]
    public void AcceptsUppercasePdf()
    {
        var upload = new DocumentUpload("REPORT.PDF", "application/pdf", 100, new MemoryStream());
        var errors = DocumentValidator.Validate(upload, _options);
        Assert.Empty(errors);
    }
    
    [Fact]
    public void ReturnsMultipleErrors()
    {
        var upload = new DocumentUpload("test.exe", "application/json", 2000000, new MemoryStream());
        var errors = DocumentValidator.Validate(upload, _options);
        Assert.Equal(3, errors.Count);
    }
}
