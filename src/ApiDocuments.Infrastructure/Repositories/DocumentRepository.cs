using ApiDocuments.Core.Interfaces;
using ApiDocuments.Core.Models;
using ApiDocuments.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiDocuments.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for <see cref="Document"/> and <see cref="DocumentAudit"/> data access.
/// </summary>
public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initialises a new instance of <see cref="DocumentRepository"/>.
    /// </summary>
    /// <param name="context">The database context to use.</param>
    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .AsNoTracking()
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await _context.Documents.AddAsync(document, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAuditAsync(DocumentAudit audit, CancellationToken cancellationToken = default)
    {
        await _context.DocumentAudits.AddAsync(audit, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DocumentAudit>> GetAuditsByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.DocumentAudits
            .AsNoTracking()
            .Where(a => a.DocumentId == documentId)
            .OrderBy(a => a.PerformedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
