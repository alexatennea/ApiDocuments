namespace ApiDocuments.Core.DTOs;

/// <summary>
/// Encapsulates the data required to upload a document.
/// </summary>
public class UploadDocumentRequest
{
    /// <summary>Gets or sets the original file name.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the MIME content type of the document.</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Gets or sets the file content stream.</summary>
    public Stream Content { get; set; } = Stream.Null;

    /// <summary>Gets or sets the size of the file in bytes.</summary>
    public long FileSizeBytes { get; set; }
}
