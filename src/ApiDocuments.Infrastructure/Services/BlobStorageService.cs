using ApiDocuments.Core.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ApiDocuments.Infrastructure.Services;

/// <summary>
/// Implements blob storage operations using Azure Blob Storage.
/// </summary>
public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    /// <summary>
    /// Initialises a new instance of <see cref="BlobStorageService"/>.
    /// </summary>
    /// <param name="containerClient">The Azure Blob container client to use.</param>
    public BlobStorageService(BlobContainerClient containerClient)
    {
        _containerClient = containerClient;
    }

    /// <inheritdoc />
    public async Task<string> UploadAsync(string blobName, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };

        await blobClient.UploadAsync(content, uploadOptions, cancellationToken);
        return blobClient.Uri.ToString();
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
