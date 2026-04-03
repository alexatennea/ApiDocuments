using ApiDocuments.Core.Models;
using ApiDocuments.Infrastructure.Data;
using ApiDocuments.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ApiDocuments.Tests.Repositories;

/// <summary>
/// Unit tests for <see cref="DocumentRepository"/> using an in-memory database.
/// </summary>
public class DocumentRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DocumentRepository _repository;

    /// <summary>Initialises a new instance of <see cref="DocumentRepositoryTests"/>.</summary>
    public DocumentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new DocumentRepository(_context);
    }

    /// <inheritdoc />
    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task AddAsync_PersistsDocument()
    {
        // Arrange
        var doc = CreateDocument();

        // Act
        await _repository.AddAsync(doc);

        // Assert
        var stored = await _context.Documents.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.Id == doc.Id);
        Assert.NotNull(stored);
        Assert.Equal(doc.FileName, stored.FileName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsDocument()
    {
        // Arrange
        var doc = CreateDocument();
        await _context.Documents.AddAsync(doc);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(doc.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(doc.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenDeleted_ReturnsNull()
    {
        // Arrange
        var doc = CreateDocument(isDeleted: true);
        await _context.Documents.AddAsync(doc);
        await _context.SaveChangesAsync();

        // Act - query filter should exclude soft-deleted records
        var result = await _repository.GetByIdAsync(doc.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyNonDeletedDocuments()
    {
        // Arrange
        var active = CreateDocument();
        var deleted = CreateDocument(isDeleted: true);
        await _context.Documents.AddRangeAsync(active, deleted);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(active.Id, result[0].Id);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        // Arrange
        var doc = CreateDocument();
        await _context.Documents.AddAsync(doc);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        doc.FileName = "updated.pdf";
        await _repository.UpdateAsync(doc);

        // Assert
        _context.ChangeTracker.Clear();
        var updated = await _context.Documents.FindAsync(doc.Id);
        Assert.Equal("updated.pdf", updated!.FileName);
    }

    [Fact]
    public async Task AddAuditAsync_PersistsAudit()
    {
        // Arrange
        var doc = CreateDocument();
        await _context.Documents.AddAsync(doc);
        await _context.SaveChangesAsync();

        var audit = new DocumentAudit
        {
            Id = Guid.NewGuid(),
            DocumentId = doc.Id,
            Action = "Uploaded",
            PerformedAtUtc = DateTime.UtcNow,
            PerformedBy = "testuser"
        };

        // Act
        await _repository.AddAuditAsync(audit);

        // Assert
        var stored = await _context.DocumentAudits.FindAsync(audit.Id);
        Assert.NotNull(stored);
        Assert.Equal("Uploaded", stored.Action);
    }

    [Fact]
    public async Task GetAuditsByDocumentIdAsync_ReturnsAuditsInOrder()
    {
        // Arrange
        var doc = CreateDocument();
        await _context.Documents.AddAsync(doc);

        var earlier = new DocumentAudit
        {
            Id = Guid.NewGuid(),
            DocumentId = doc.Id,
            Action = "Uploaded",
            PerformedAtUtc = DateTime.UtcNow.AddMinutes(-10),
            PerformedBy = "user1"
        };
        var later = new DocumentAudit
        {
            Id = Guid.NewGuid(),
            DocumentId = doc.Id,
            Action = "Downloaded",
            PerformedAtUtc = DateTime.UtcNow,
            PerformedBy = "user2"
        };

        await _context.DocumentAudits.AddRangeAsync(later, earlier); // intentionally reversed
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAuditsByDocumentIdAsync(doc.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Uploaded", result[0].Action);
        Assert.Equal("Downloaded", result[1].Action);
    }

    private static Document CreateDocument(bool isDeleted = false) => new()
    {
        Id = Guid.NewGuid(),
        FileName = "sample.pdf",
        ContentType = "application/pdf",
        BlobPath = "container/sample.pdf",
        FileSizeBytes = 1024,
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow,
        IsDeleted = isDeleted
    };
}
