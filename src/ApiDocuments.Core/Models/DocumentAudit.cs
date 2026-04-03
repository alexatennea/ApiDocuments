namespace ApiDocuments.Core.Models;

/// <summary>
/// Represents an audit entry that tracks changes to a <see cref="Document"/>.
/// </summary>
public class DocumentAudit
{
    /// <summary>Gets or sets the unique identifier of the audit entry.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the identifier of the document this audit entry belongs to.</summary>
    public Guid DocumentId { get; set; }

    /// <summary>Gets or sets the action performed on the document (e.g. Uploaded, Downloaded, Deleted).</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Gets or sets additional details about the action performed.</summary>
    public string? Details { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the action was performed.</summary>
    public DateTime PerformedAtUtc { get; set; }

    /// <summary>Gets or sets the identifier of the user or system that performed the action.</summary>
    public string PerformedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets the document this audit entry belongs to.</summary>
    public Document Document { get; set; } = null!;
}
