using ApiDocuments.Core.DTOs;
using ApiDocuments.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiDocuments.Api.Controllers;

/// <summary>
/// Provides endpoints for uploading, retrieving, and deleting documents.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    /// <summary>
    /// Initialises a new instance of <see cref="DocumentsController"/>.
    /// </summary>
    /// <param name="documentService">The document service.</param>
    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    /// <summary>
    /// Returns a list of all available documents.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>A list of <see cref="DocumentDto"/> objects.</returns>
    /// <response code="200">Returns the list of documents.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DocumentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var documents = await _documentService.ListDocumentsAsync(cancellationToken);
        return Ok(documents);
    }

    /// <summary>
    /// Returns the metadata for a specific document.
    /// </summary>
    /// <param name="id">The unique identifier of the document.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>A <see cref="DocumentDto"/> for the requested document.</returns>
    /// <response code="200">Returns the document metadata.</response>
    /// <response code="404">Document not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var document = await _documentService.GetDocumentAsync(id, cancellationToken);
        return document is null ? NotFound() : Ok(document);
    }

    /// <summary>
    /// Downloads the binary content of a document.
    /// </summary>
    /// <param name="id">The unique identifier of the document.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The document file.</returns>
    /// <response code="200">Returns the document file.</response>
    /// <response code="404">Document not found.</response>
    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var performedBy = User.Identity?.Name ?? "anonymous";
        var result = await _documentService.DownloadDocumentAsync(id, performedBy, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    /// <summary>
    /// Uploads a new document.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The <see cref="DocumentDto"/> for the newly created document.</returns>
    /// <response code="201">Document uploaded successfully.</response>
    /// <response code="400">No file was provided or the file is empty.</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("A non-empty file must be provided.");
        }

        var performedBy = User.Identity?.Name ?? "anonymous";
        var uploadRequest = new UploadDocumentRequest
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Content = file.OpenReadStream(),
            FileSizeBytes = file.Length
        };

        var document = await _documentService.UploadDocumentAsync(uploadRequest, performedBy, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = document.Id }, document);
    }

    /// <summary>
    /// Deletes a document.
    /// </summary>
    /// <param name="id">The unique identifier of the document to delete.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>No content if the deletion succeeded.</returns>
    /// <response code="204">Document deleted successfully.</response>
    /// <response code="404">Document not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var performedBy = User.Identity?.Name ?? "anonymous";
        var deleted = await _documentService.DeleteDocumentAsync(id, performedBy, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// Returns the audit history for a specific document.
    /// </summary>
    /// <param name="id">The unique identifier of the document.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>A list of <see cref="DocumentAuditDto"/> entries.</returns>
    /// <response code="200">Returns the audit history.</response>
    /// <response code="404">Document not found.</response>
    [HttpGet("{id:guid}/audit")]
    [ProducesResponseType(typeof(IReadOnlyList<DocumentAuditDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAudit(Guid id, CancellationToken cancellationToken)
    {
        var audits = await _documentService.GetDocumentAuditAsync(id, cancellationToken);
        return audits is null ? NotFound() : Ok(audits);
    }
}
