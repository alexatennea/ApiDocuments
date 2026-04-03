using ApiDocuments.Core.Models;

namespace ApiDocuments.Core.Interfaces;

/// <summary>
/// Defines the contract for document data-access operations.
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Retrieves a document by its unique identifier.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The <see cref="Document"/> if found; otherwise <c>null</c>.</returns>
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all non-deleted documents.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A read-only list of <see cref="Document"/> entities.</returns>
    Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new document to the data store.
    /// </summary>
    /// <param name="document">The document to add.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task AddAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing document in the data store.
    /// </summary>
    /// <param name="document">The document with updated values.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an audit entry to the data store.
    /// </summary>
    /// <param name="audit">The audit entry to add.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task AddAuditAsync(DocumentAudit audit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all audit entries for a given document.
    /// </summary>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A read-only list of <see cref="DocumentAudit"/> entries.</returns>
    Task<IReadOnlyList<DocumentAudit>> GetAuditsByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
}
