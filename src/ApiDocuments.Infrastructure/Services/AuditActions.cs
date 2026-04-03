namespace ApiDocuments.Infrastructure.Services;

/// <summary>
/// Contains constants for audit action names used throughout the application.
/// </summary>
public static class AuditActions
{
    /// <summary>Action recorded when a document is uploaded.</summary>
    public const string Uploaded = "Uploaded";

    /// <summary>Action recorded when a document is downloaded.</summary>
    public const string Downloaded = "Downloaded";

    /// <summary>Action recorded when a document is deleted.</summary>
    public const string Deleted = "Deleted";
}
