using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Wp1Fall26Aws.Storage;
using Xunit;

namespace Wp1Fall26Aws.Tests;

public sealed class DocumentApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DocumentApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostDocuments_ValidFile_Returns201AndCallsS3()
    {
        // Arrange: setup the spy and test client
        var spy = new RecordingS3ObjectClientSpy();
        var client = _factory.WithWebHostBuilder(b =>
        {
            b.ConfigureServices(services =>
            {
                services.AddSingleton<IS3ObjectClient>(spy);
            });
        }).CreateClient();

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent("synthetic valid content"u8.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "test.txt");

        // Act
        var response = await client.PostAsync("/documents", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.Contains("objectKey", responseBody);

        Assert.NotNull(spy.LastRequest);
        Assert.Equal("test.txt", spy.LastRequest.Metadata["original-filename"]);
    }
}
