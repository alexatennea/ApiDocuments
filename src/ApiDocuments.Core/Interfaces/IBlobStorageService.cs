namespace ApiDocuments.Core.Interfaces;

/// <summary>
/// Defines the contract for blob storage operations.
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Uploads a stream to blob storage.
    /// </summary>
    /// <param name="blobName">The name (path) of the blob within the container.</param>
    /// <param name="content">The stream containing the blob content.</param>
    /// <param name="contentType">The MIME type of the content.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The URI of the uploaded blob.</returns>
    Task<string> UploadAsync(string blobName, Stream content, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a blob as a stream.
    /// </summary>
    /// <param name="blobName">The name (path) of the blob within the container.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="Stream"/> containing the blob content.</returns>
    Task<Stream> DownloadAsync(string blobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a blob from storage.
    /// </summary>
    /// <param name="blobName">The name (path) of the blob within the container.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task DeleteAsync(string blobName, CancellationToken cancellationToken = default);
}
