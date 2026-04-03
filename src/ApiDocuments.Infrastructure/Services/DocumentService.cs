using ApiDocuments.Core.DTOs;
using ApiDocuments.Core.Interfaces;
using ApiDocuments.Core.Models;
using Microsoft.Extensions.Logging;

namespace ApiDocuments.Infrastructure.Services;

/// <summary>
/// Implements document business-logic operations.
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly IBlobStorageService _blobStorage;
    private readonly ILogger<DocumentService> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="DocumentService"/>.
    /// </summary>
    /// <param name="repository">The document repository.</param>
    /// <param name="blobStorage">The blob storage service.</param>
    /// <param name="logger">The logger.</param>
    public DocumentService(
        IDocumentRepository repository,
        IBlobStorageService blobStorage,
        ILogger<DocumentService> logger)
    {
        _repository = repository;
        _blobStorage = blobStorage;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<DocumentDto> UploadDocumentAsync(UploadDocumentRequest request, string performedBy, CancellationToken cancellationToken = default)
    {
        var documentId = Guid.NewGuid();
        var blobName = $"{documentId}/{request.FileName}";

        _logger.LogInformation("Uploading document '{FileName}' for user '{User}'", request.FileName, performedBy);

        await _blobStorage.UploadAsync(blobName, request.Content, request.ContentType, cancellationToken);

        var now = DateTime.UtcNow;
        var document = new Document
        {
            Id = documentId,
            FileName = request.FileName,
            ContentType = request.ContentType,
            BlobPath = blobName,
            FileSizeBytes = request.FileSizeBytes,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            IsDeleted = false
        };

        await _repository.AddAsync(document, cancellationToken);

        await _repository.AddAuditAsync(new DocumentAudit
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            Action = AuditActions.Uploaded,
            Details = $"File size: {request.FileSizeBytes} bytes",
            PerformedAtUtc = now,
            PerformedBy = performedBy
        }, cancellationToken);

        _logger.LogInformation("Document '{DocumentId}' uploaded successfully", documentId);

        return MapToDto(document);
    }

    /// <inheritdoc />
    public async Task<DocumentDto?> GetDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _repository.GetByIdAsync(id, cancellationToken);
        return document is null ? null : MapToDto(document);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DocumentDto>> ListDocumentsAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _repository.GetAllAsync(cancellationToken);
        return documents.Select(MapToDto).ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<(Stream Content, string ContentType, string FileName)?> DownloadDocumentAsync(Guid id, string performedBy, CancellationToken cancellationToken = default)
    {
        var document = await _repository.GetByIdAsync(id, cancellationToken);
        if (document is null)
        {
            return null;
        }

        _logger.LogInformation("Downloading document '{DocumentId}' for user '{User}'", id, performedBy);

        var stream = await _blobStorage.DownloadAsync(document.BlobPath, cancellationToken);

        await _repository.AddAuditAsync(new DocumentAudit
        {
            Id = Guid.NewGuid(),
            DocumentId = id,
            Action = AuditActions.Downloaded,
            PerformedAtUtc = DateTime.UtcNow,
            PerformedBy = performedBy
        }, cancellationToken);

        return (stream, document.ContentType, document.FileName);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteDocumentAsync(Guid id, string performedBy, CancellationToken cancellationToken = default)
    {
        var document = await _repository.GetByIdAsync(id, cancellationToken);
        if (document is null)
        {
            return false;
        }

        _logger.LogInformation("Deleting document '{DocumentId}' for user '{User}'", id, performedBy);

        document.IsDeleted = true;
        document.UpdatedAtUtc = DateTime.UtcNow;

        await _repository.UpdateAsync(document, cancellationToken);

        await _blobStorage.DeleteAsync(document.BlobPath, cancellationToken);

        await _repository.AddAuditAsync(new DocumentAudit
        {
            Id = Guid.NewGuid(),
            DocumentId = id,
            Action = AuditActions.Deleted,
            PerformedAtUtc = DateTime.UtcNow,
            PerformedBy = performedBy
        }, cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DocumentAuditDto>?> GetDocumentAuditAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _repository.GetByIdAsync(id, cancellationToken);
        if (document is null)
        {
            return null;
        }

        var audits = await _repository.GetAuditsByDocumentIdAsync(id, cancellationToken);
        return audits.Select(a => new DocumentAuditDto
        {
            Id = a.Id,
            DocumentId = a.DocumentId,
            Action = a.Action,
            Details = a.Details,
            PerformedAtUtc = a.PerformedAtUtc,
            PerformedBy = a.PerformedBy
        }).ToList().AsReadOnly();
    }

    private static DocumentDto MapToDto(Document document) => new()
    {
        Id = document.Id,
        FileName = document.FileName,
        ContentType = document.ContentType,
        FileSizeBytes = document.FileSizeBytes,
        CreatedAtUtc = document.CreatedAtUtc,
        UpdatedAtUtc = document.UpdatedAtUtc
    };
}
