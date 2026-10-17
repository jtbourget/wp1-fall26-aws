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


// =============================================================================
// Application Initialization & Host Configuration
// =============================================================================
var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// AWS SDK & Credentials Configuration
// -----------------------------------------------------------------------------
// Resolves AWS options from configuration or environment variables.
// In local development or automated CI runners where real AWS credentials
// are deliberately not supplied, we configure a safe fallback mock credential
// and default region (us-east-1) to allow the AWS SDK client to instantiate
// without throwing initialization exceptions.
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

// Register storage options, S3 boundary client, document store, and repository
builder.Services.AddStorageServices(builder.Configuration);

// -----------------------------------------------------------------------------
// Cross-Origin Resource Sharing (CORS) Policy
// -----------------------------------------------------------------------------
// Enables cross-origin HTTP requests from client applications (such as the
// Angular consumer app running on http://localhost:4200 or custom ports).
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

// =============================================================================
// HTTP Request Pipeline Middleware
// =============================================================================
app.UseCors();

// Serve static frontend assets from wwwroot (compiled Angular SPA bundle)
app.UseDefaultFiles();
app.UseStaticFiles();

// =============================================================================
// REST API Endpoints
// =============================================================================

/// <summary>
/// Health Check Probe
/// GET /health
/// Returns service liveness status. Used by consumer clients, AWS Elastic Beanstalk
/// load balancers, and container orchestrators to verify application availability.
/// </summary>
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }))
   .WithName("HealthCheck")
   .WithSummary("Service health probe")
   .WithDescription("Returns HTTP 200 with service health status when the API is running.");

/// <summary>
/// Retrieve All Documents
/// GET /documents
/// Returns a collection of all stored document records, sorted newest first.
/// </summary>
app.MapGet("/documents", async (IDocumentRepository repository) =>
{
    var documents = await repository.GetAllAsync();
    return Results.Ok(documents);
})
.WithName("GetAllDocuments")
.WithSummary("Retrieve all document records")
.WithDescription("Queries the repository for all stored document metadata records.");

/// <summary>
/// Retrieve Single Document
/// GET /documents/{id}
/// Finds and returns metadata for an individual document matching the given ID or S3 ObjectKey.
/// Returns 404 Not Found if no document matches the identifier.
/// </summary>
app.MapGet("/documents/{id}", async (string id, IDocumentRepository repository) =>
{
    var document = await repository.GetByIdAsync(id);
    return document is not null
        ? Results.Ok(document)
        : Results.NotFound(new { error = $"Document with ID '{id}' was not found." });
})
.WithName("GetDocumentById")
.WithSummary("Retrieve an individual document by ID or S3 ObjectKey")
.WithDescription("Looks up a document in the repository by unique identifier or S3 key.");

/// <summary>
/// Create / Upload Document
/// POST /documents
/// Accepts either:
/// 1. Multipart Form Data: file (binary), title (optional), description (optional).
/// 2. JSON: title (required), description, fileName, contentType, content.
/// Uploads the binary stream to Amazon S3 and registers the document record in the repository.
/// </summary>
app.MapPost("/documents", async (HttpRequest request, IDocumentStore documentStore, IDocumentRepository repository, IOptions<S3StorageOptions> options) =>
{
    // Mode 1: Multipart form-data file upload
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
    // Mode 2: JSON payload creating a text-based document
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
})
.WithName("CreateDocument")
.WithSummary("Upload or create a new document resource")
.WithDescription("Persists a document to AWS S3 and creates an indexed metadata record.");

/// <summary>
/// Update Document Metadata
/// PUT /documents/{id}
/// Updates editable metadata (Title and Description) of an existing document record.
/// Updates the record's UpdatedUtc timestamp.
/// </summary>
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
})
.WithName("UpdateDocument")
.WithSummary("Update document metadata")
.WithDescription("Modifies Title and Description of an existing document record.");

/// <summary>
/// Delete Document
/// DELETE /documents/{id}
/// Removes the document record from the repository index.
/// </summary>
app.MapDelete("/documents/{id}", async (string id, IDocumentRepository repository) =>
{
    var deleted = await repository.DeleteAsync(id);
    if (!deleted)
    {
        return Results.NotFound(new { error = $"Document with ID '{id}' was not found." });
    }

    return Results.Ok(new { message = $"Document '{id}' deleted successfully.", id });
})
.WithName("DeleteDocument")
.WithSummary("Delete a document record")
.WithDescription("Deletes an existing document record by its unique ID.");

// Single-page application fallback route
app.MapFallbackToFile("index.html");

app.Run();

/// <summary>
/// Entry point marker class required for WebApplicationFactory integration testing.
/// </summary>
public partial class Program { }
