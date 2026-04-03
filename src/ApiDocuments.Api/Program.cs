using ApiDocuments.Core.Interfaces;
using ApiDocuments.Infrastructure.Data;
using ApiDocuments.Infrastructure.Repositories;
using ApiDocuments.Infrastructure.Services;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "ApiDocuments",
        Version = "v1",
        Description = "API for uploading, retrieving, and deleting documents."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Blob Storage ──────────────────────────────────────────────────────────────
builder.Services.AddSingleton(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("BlobStorage")
        ?? throw new InvalidOperationException("BlobStorage connection string is not configured.");
    var containerName = builder.Configuration["BlobStorage:ContainerName"] ?? "documents";
    var client = new BlobContainerClient(connectionString, containerName);
    client.CreateIfNotExists();
    return client;
});

// ── Services & Repositories ────────────────────────────────────────────────────
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>
/// Partial Program class to allow test projects to reference the application entry point.
/// </summary>
public partial class Program { }
