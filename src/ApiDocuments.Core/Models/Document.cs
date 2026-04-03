namespace ApiDocuments.Core.Models;

/// <summary>
/// Represents a document stored in the system.
/// </summary>
public class Document
{
    /// <summary>Gets or sets the unique identifier of the document.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the original file name of the document.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the MIME content type of the document.</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Gets or sets the path within blob storage where the document is stored.</summary>
    public string BlobPath { get; set; } = string.Empty;

    /// <summary>Gets or sets the size of the document in bytes.</summary>
    public long FileSizeBytes { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the document was created.</summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the document was last updated.</summary>
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>Gets or sets a value indicating whether the document has been soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Gets or sets the audit trail entries for this document.</summary>
    public ICollection<DocumentAudit> Audits { get; set; } = new List<DocumentAudit>();
}
