using ApiDocuments.Core.DTOs;

namespace ApiDocuments.Core.Interfaces;

/// <summary>
/// Defines the contract for document business-logic operations.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Uploads a document and stores its metadata.
    /// </summary>
    /// <param name="request">The upload request containing file data.</param>
    /// <param name="performedBy">The identifier of the user performing the upload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The <see cref="DocumentDto"/> representing the newly created document.</returns>
    Task<DocumentDto> UploadDocumentAsync(UploadDocumentRequest request, string performedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the metadata for a document by its identifier.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The <see cref="DocumentDto"/> if found; otherwise <c>null</c>.</returns>
    Task<DocumentDto?> GetDocumentAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all available (non-deleted) documents.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A read-only list of <see cref="DocumentDto"/> objects.</returns>
    Task<IReadOnlyList<DocumentDto>> ListDocumentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the binary content of a document.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="performedBy">The identifier of the user performing the download.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A tuple containing the content <see cref="Stream"/>, the content type string, and the file name;
    /// or <c>null</c> if the document was not found.
    /// </returns>
    Task<(Stream Content, string ContentType, string FileName)?> DownloadDocumentAsync(Guid id, string performedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a document.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="performedBy">The identifier of the user performing the deletion.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><c>true</c> if the document was found and deleted; otherwise <c>false</c>.</returns>
    Task<bool> DeleteDocumentAsync(Guid id, string performedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all audit entries for a document.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A read-only list of <see cref="DocumentAuditDto"/> objects; or <c>null</c> if the document was not found.</returns>
    Task<IReadOnlyList<DocumentAuditDto>?> GetDocumentAuditAsync(Guid id, CancellationToken cancellationToken = default);
}
