using System;
using System.Linq;
using Amazon.S3;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wp1Fall26Aws.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddStorageServices(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "OK");
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

app.MapPost("/documents", async (HttpRequest request, IDocumentStore documentStore) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest(new { errors = new[] { "No file was provided." } });
    }

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

        return Results.Created($"/documents/{result.ObjectKey}", new { objectKey = result.ObjectKey, metadata = result.Metadata });
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Storage failed");
        return Results.StatusCode(500);
    }
});

app.Run();

public partial class Program { }
