namespace ApiDocuments.Core.DTOs;

/// <summary>
/// Data transfer object representing document metadata returned to callers.
/// </summary>
public class DocumentDto
{
    /// <summary>Gets or sets the unique identifier of the document.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the original file name of the document.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the MIME content type of the document.</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Gets or sets the size of the document in bytes.</summary>
    public long FileSizeBytes { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the document was created.</summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the document was last updated.</summary>
    public DateTime UpdatedAtUtc { get; set; }
}
