using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

    [Fact]
    public async Task GetDocuments_Returns200WithList()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/documents");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("doc-1", body);
    }

    [Fact]
    public async Task PutDocument_ValidUpdate_Returns200WithUpdatedData()
    {
        var client = _factory.CreateClient();

        var updatePayload = new UpdateDocumentRequest("Updated Title", "Updated Description");
        var response = await client.PutAsJsonAsync("/documents/doc-1", updatePayload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Updated Title", body);
    }

    [Fact]
    public async Task DeleteDocument_ExistingItem_Returns200AndRemovesItem()
    {
        var client = _factory.CreateClient();

        // Delete doc-2
        var deleteResponse = await client.DeleteAsync("/documents/doc-2");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // Verify doc-2 is gone
        var getResponse = await client.GetAsync("/documents/doc-2");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
