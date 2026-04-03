namespace ApiDocuments.Core.DTOs;

/// <summary>
/// Data transfer object representing an audit entry for a document action.
/// </summary>
public class DocumentAuditDto
{
    /// <summary>Gets or sets the unique identifier of the audit entry.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the identifier of the document this entry relates to.</summary>
    public Guid DocumentId { get; set; }

    /// <summary>Gets or sets the action that was performed (e.g. Uploaded, Downloaded, Deleted).</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Gets or sets additional details about the action.</summary>
    public string? Details { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the action was performed.</summary>
    public DateTime PerformedAtUtc { get; set; }

    /// <summary>Gets or sets the identifier of the user or system that performed the action.</summary>
    public string PerformedBy { get; set; } = string.Empty;
}
