using System.IO;
using System.Linq;
using Wp1Fall26Aws.Storage;
using Xunit;

namespace Wp1Fall26Aws.Tests;

/// <summary>
/// Unit tests verifying document upload validation policy rules.
/// </summary>
/// <remarks>
/// Validates boundary limits (zero-byte, 1 MiB boundary, 1 byte over 1 MiB),
/// file extension allowlisting/case insensitivity, MIME content type restrictions,
/// and multiple error aggregation.
/// </remarks>
public class DocumentValidatorTests
{
    private readonly S3StorageOptions _options = new() { MaxFileSizeBytes = 1048576 };

    /// <summary>
    /// Verifies that a valid small .txt document produces zero validation errors.
    /// </summary>
    [Fact]
    public void AcceptsValidSmallTxt()
    {
        var upload = new DocumentUpload("test.txt", "text/plain", 100, new MemoryStream(new byte[100]));
        var errors = DocumentValidator.Validate(upload, _options);
        Assert.Empty(errors);
    }

    /// <summary>
    /// Verifies that a 0-byte (empty) file is rejected with the message "File is empty.".
    /// </summary>
    [Fact]
    public void RejectsEmpty()
    {
        var upload = new DocumentUpload("test.txt", "text/plain", 0, new MemoryStream());
        var errors = DocumentValidator.Validate(upload, _options);
        Assert.Contains(errors, e => e == "File is empty.");
    }

    /// <summary>
    /// Verifies precise boundary behavior: exactly 1,048,576 bytes is accepted,
    /// while 1,048,577 bytes (1 byte over boundary) is rejected.
    /// </summary>
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

    /// <summary>
    /// Verifies that disallowed executable extensions (.exe) are rejected.
    /// </summary>
    [Fact]
    public void RejectsExe()
    {
        var upload = new DocumentUpload("test.exe", "text/plain", 100, new MemoryStream());
        var errors = DocumentValidator.Validate(upload, _options);
        Assert.Contains(errors, e => e.Contains("not allowed"));
    }

    /// <summary>
    /// Verifies that disallowed MIME content types (such as application/json) are rejected.
    /// </summary>
    [Fact]
    public void RejectsDisallowedContentType()
    {
        var upload = new DocumentUpload("test.txt", "application/json", 100, new MemoryStream());
        var errors = DocumentValidator.Validate(upload, _options);
        Assert.Contains(errors, e => e.Contains("not allowed"));
    }

    /// <summary>
    /// Verifies that file extension checking is case-insensitive (e.g. uppercase .PDF is accepted).
    /// </summary>
    [Fact]
    public void AcceptsUppercasePdf()
    {
        var upload = new DocumentUpload("REPORT.PDF", "application/pdf", 100, new MemoryStream());
        var errors = DocumentValidator.Validate(upload, _options);
        Assert.Empty(errors);
    }
    
    /// <summary>
    /// Verifies that multiple validation errors (e.g. bad extension + bad MIME + oversize)
    /// are aggregated into the returned error list simultaneously.
    /// </summary>
    [Fact]
    public void ReturnsMultipleErrors()
    {
        var upload = new DocumentUpload("test.exe", "application/json", 2000000, new MemoryStream());
        var errors = DocumentValidator.Validate(upload, _options);
        Assert.Equal(3, errors.Count);
    }
}
