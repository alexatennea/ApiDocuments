using ApiDocuments.Core.DTOs;
using ApiDocuments.Core.Interfaces;
using ApiDocuments.Core.Models;
using ApiDocuments.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ApiDocuments.Tests.Services;

/// <summary>
/// Unit tests for <see cref="DocumentService"/>.
/// </summary>
public class DocumentServiceTests
{
    private readonly Mock<IDocumentRepository> _repositoryMock;
    private readonly Mock<IBlobStorageService> _blobMock;
    private readonly DocumentService _service;

    /// <summary>Initialises a new instance of <see cref="DocumentServiceTests"/>.</summary>
    public DocumentServiceTests()
    {
        _repositoryMock = new Mock<IDocumentRepository>();
        _blobMock = new Mock<IBlobStorageService>();
        _service = new DocumentService(
            _repositoryMock.Object,
            _blobMock.Object,
            NullLogger<DocumentService>.Instance);
    }

    [Fact]
    public async Task UploadDocumentAsync_SavesDocumentAndAudit()
    {
        // Arrange
        var request = new UploadDocumentRequest
        {
            FileName = "report.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 2048,
            Content = new MemoryStream(new byte[2048])
        };

        _blobMock.Setup(b => b.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), "application/pdf", It.IsAny<CancellationToken>()))
                 .ReturnsAsync("https://blob/container/blob");

        // Act
        var result = await _service.UploadDocumentAsync(request, "user1");

        // Assert
        Assert.Equal("report.pdf", result.FileName);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal(2048, result.FileSizeBytes);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.AddAuditAsync(
            It.Is<DocumentAudit>(a => a.Action == AuditActions.Uploaded),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDocumentAsync_WhenExists_ReturnsMappedDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var doc = new Document
        {
            Id = id,
            FileName = "invoice.pdf",
            ContentType = "application/pdf",
            BlobPath = $"{id}/invoice.pdf",
            FileSizeBytes = 512,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(doc);

        // Act
        var result = await _service.GetDocumentAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("invoice.pdf", result.FileName);
    }

    [Fact]
    public async Task GetDocumentAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((Document?)null);

        // Act
        var result = await _service.GetDocumentAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ListDocumentsAsync_ReturnsMappedList()
    {
        // Arrange
        var docs = new List<Document>
        {
            new() { Id = Guid.NewGuid(), FileName = "a.pdf", ContentType = "application/pdf", CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), FileName = "b.docx", ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document", CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow }
        };

        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(docs.AsReadOnly());

        // Act
        var result = await _service.ListDocumentsAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task DownloadDocumentAsync_WhenExists_ReturnsStreamAndAudit()
    {
        // Arrange
        var id = Guid.NewGuid();
        var doc = new Document
        {
            Id = id,
            FileName = "contract.pdf",
            ContentType = "application/pdf",
            BlobPath = $"{id}/contract.pdf",
            FileSizeBytes = 1024,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        var contentStream = new MemoryStream(new byte[1024]);

        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(doc);
        _blobMock.Setup(b => b.DownloadAsync(doc.BlobPath, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(contentStream);

        // Act
        var result = await _service.DownloadDocumentAsync(id, "user1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("application/pdf", result.Value.ContentType);
        Assert.Equal("contract.pdf", result.Value.FileName);

        _repositoryMock.Verify(r => r.AddAuditAsync(
            It.Is<DocumentAudit>(a => a.Action == AuditActions.Downloaded),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DownloadDocumentAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((Document?)null);

        // Act
        var result = await _service.DownloadDocumentAsync(Guid.NewGuid(), "user1");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteDocumentAsync_WhenExists_SoftDeletesAndAudits()
    {
        // Arrange
        var id = Guid.NewGuid();
        var doc = new Document
        {
            Id = id,
            FileName = "old.pdf",
            ContentType = "application/pdf",
            BlobPath = $"{id}/old.pdf",
            FileSizeBytes = 256,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            IsDeleted = false
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(doc);

        // Act
        var result = await _service.DeleteDocumentAsync(id, "user2");

        // Assert
        Assert.True(result);
        Assert.True(doc.IsDeleted);

        _repositoryMock.Verify(r => r.UpdateAsync(doc, It.IsAny<CancellationToken>()), Times.Once);
        _blobMock.Verify(b => b.DeleteAsync(doc.BlobPath, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.AddAuditAsync(
            It.Is<DocumentAudit>(a => a.Action == AuditActions.Deleted),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteDocumentAsync_WhenNotFound_ReturnsFalse()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((Document?)null);

        // Act
        var result = await _service.DeleteDocumentAsync(Guid.NewGuid(), "user2");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetDocumentAuditAsync_WhenExists_ReturnsAudits()
    {
        // Arrange
        var id = Guid.NewGuid();
        var doc = new Document { Id = id, FileName = "x.pdf", ContentType = "application/pdf", CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow };
        var audits = new List<DocumentAudit>
        {
            new() { Id = Guid.NewGuid(), DocumentId = id, Action = "Uploaded", PerformedAtUtc = DateTime.UtcNow, PerformedBy = "user1" }
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);
        _repositoryMock.Setup(r => r.GetAuditsByDocumentIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(audits.AsReadOnly());

        // Act
        var result = await _service.GetDocumentAuditAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Uploaded", result[0].Action);
    }

    [Fact]
    public async Task GetDocumentAuditAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((Document?)null);

        // Act
        var result = await _service.GetDocumentAuditAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }
}
