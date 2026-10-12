using System;
using System.IO;
using System.Linq;
using Amazon.S3;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wp1Fall26Aws.Storage;

var builder = WebApplication.CreateBuilder(args);

var awsOptions = builder.Configuration.GetAWSOptions();
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")) &&
    string.IsNullOrEmpty(builder.Configuration["AWS:AccessKey"]))
{
    awsOptions.Credentials = new Amazon.Runtime.BasicAWSCredentials("local-dev-mock", "local-dev-mock");
    if (awsOptions.Region == null)
    {
        awsOptions.Region = Amazon.RegionEndpoint.USEast1;
    }
}
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddStorageServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

app.MapGet("/", () => "OK");
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

// GET /documents: Retrieve all stored documents
app.MapGet("/documents", async (IDocumentRepository repository) =>
{
    var documents = await repository.GetAllAsync();
    return Results.Ok(documents);
});

// GET /documents/{id}: Retrieve document by ID or ObjectKey
app.MapGet("/documents/{id}", async (string id, IDocumentRepository repository) =>
{
    var document = await repository.GetByIdAsync(id);
    return document is not null
        ? Results.Ok(document)
        : Results.NotFound(new { error = $"Document with ID '{id}' was not found." });
});

// POST /documents: Upload a file (multipart) or create a document (JSON)
app.MapPost("/documents", async (HttpRequest request, IDocumentStore documentStore, IDocumentRepository repository, IOptions<S3StorageOptions> options) =>
{
    if (request.HasFormContentType)
    {
        var form = await request.ReadFormAsync();
        var file = form.Files.GetFile("file") ?? form.Files.FirstOrDefault();

        if (file == null)
        {
            return Results.BadRequest(new { errors = new[] { "No file was provided." } });
        }

        using var stream = file.OpenReadStream();
        var upload = new DocumentUpload(file.FileName, file.ContentType, file.Length, stream);

        try
        {
            var result = await documentStore.StoreAsync(upload);

            if (!result.Success)
            {
                return Results.BadRequest(new { errors = result.Errors });
            }

            var title = form["title"].ToString();
            if (string.IsNullOrWhiteSpace(title))
            {
                title = Path.GetFileNameWithoutExtension(file.FileName);
            }

            var description = form["description"].ToString();
            if (string.IsNullOrWhiteSpace(description))
            {
                description = $"Uploaded {file.FileName} ({file.Length} bytes)";
            }

            var docId = Guid.NewGuid().ToString("N");
            var record = new DocumentRecord(
                Id: docId,
                ObjectKey: result.ObjectKey!,
                FileName: file.FileName,
                ContentType: file.ContentType,
                ContentLength: file.Length,
                UploadedUtc: DateTime.UtcNow,
                Title: title,
                Description: description,
                Metadata: result.Metadata
            );

            await repository.AddAsync(record);

            return Results.Created($"/documents/{result.ObjectKey}", new
            {
                id = record.Id,
                objectKey = result.ObjectKey,
                metadata = result.Metadata,
                title = record.Title,
                description = record.Description,
                fileName = record.FileName,
                contentType = record.ContentType,
                contentLength = record.ContentLength,
                uploadedUtc = record.UploadedUtc
            });
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Storage failed");
            return Results.StatusCode(500);
        }
    }
    else if (request.HasJsonContentType())
    {
        try
        {
            var body = await request.ReadFromJsonAsync<CreateDocumentRequest>();
            if (body == null || string.IsNullOrWhiteSpace(body.Title))
            {
                return Results.BadRequest(new { errors = new[] { "Title is required." } });
            }

            var fileName = string.IsNullOrWhiteSpace(body.FileName) ? $"{Guid.NewGuid():N}.txt" : body.FileName;
            var contentType = string.IsNullOrWhiteSpace(body.ContentType) ? "text/plain" : body.ContentType;
            var textContent = body.Content ?? body.Description ?? "Sample content";
            var contentBytes = System.Text.Encoding.UTF8.GetBytes(textContent);

            using var stream = new MemoryStream(contentBytes);
            var upload = new DocumentUpload(fileName, contentType, contentBytes.Length, stream);

            var result = await documentStore.StoreAsync(upload);
            if (!result.Success)
            {
                return Results.BadRequest(new { errors = result.Errors });
            }

            var docId = Guid.NewGuid().ToString("N");
            var record = new DocumentRecord(
                Id: docId,
                ObjectKey: result.ObjectKey!,
                FileName: fileName,
                ContentType: contentType,
                ContentLength: contentBytes.Length,
                UploadedUtc: DateTime.UtcNow,
                Title: body.Title,
                Description: body.Description ?? string.Empty,
                Metadata: result.Metadata
            );

            await repository.AddAsync(record);

            return Results.Created($"/documents/{result.ObjectKey}", new
            {
                id = record.Id,
                objectKey = result.ObjectKey,
                metadata = result.Metadata,
                title = record.Title,
                description = record.Description,
                fileName = record.FileName,
                contentType = record.ContentType,
                contentLength = record.ContentLength,
                uploadedUtc = record.UploadedUtc
            });
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Document creation failed");
            return Results.StatusCode(500);
        }
    }

    return Results.BadRequest(new { errors = new[] { "Invalid content type. Expected multipart/form-data or application/json." } });
});

// PUT /documents/{id}: Update document metadata
app.MapPut("/documents/{id}", async (string id, UpdateDocumentRequest updateRequest, IDocumentRepository repository) =>
{
    if (string.IsNullOrWhiteSpace(updateRequest.Title))
    {
        return Results.BadRequest(new { errors = new[] { "Title cannot be empty." } });
    }

    var updated = await repository.UpdateAsync(id, updateRequest);
    if (updated is null)
    {
        return Results.NotFound(new { error = $"Document with ID '{id}' was not found." });
    }

    return Results.Ok(updated);
});

// DELETE /documents/{id}: Delete a document
app.MapDelete("/documents/{id}", async (string id, IDocumentRepository repository) =>
{
    var deleted = await repository.DeleteAsync(id);
    if (!deleted)
    {
        return Results.NotFound(new { error = $"Document with ID '{id}' was not found." });
    }

    return Results.Ok(new { message = $"Document '{id}' deleted successfully.", id });
});

app.Run();

public partial class Program { }
