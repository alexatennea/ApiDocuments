using ApiDocuments.Core.DTOs;
using ApiDocuments.Core.Interfaces;
using ApiDocuments.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ApiDocuments.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="DocumentsController"/>.
/// </summary>
public class DocumentsControllerTests
{
    private readonly Mock<IDocumentService> _serviceMock;
    private readonly DocumentsController _controller;

    /// <summary>Initialises a new instance of <see cref="DocumentsControllerTests"/>.</summary>
    public DocumentsControllerTests()
    {
        _serviceMock = new Mock<IDocumentService>();
        _controller = new DocumentsController(_serviceMock.Object);

        // Set up a default HTTP context so User.Identity is available
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithDocuments()
    {
        // Arrange
        var docs = new List<DocumentDto>
        {
            new() { Id = Guid.NewGuid(), FileName = "test.pdf", ContentType = "application/pdf" }
        }.AsReadOnly();

        _serviceMock.Setup(s => s.ListDocumentsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(docs);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(docs, ok.Value);
    }

    [Fact]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new DocumentDto { Id = id, FileName = "file.txt", ContentType = "text/plain" };

        _serviceMock.Setup(s => s.GetDocumentAsync(id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dto);

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetDocumentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((DocumentDto?)null);

        // Act
        var result = await _controller.GetById(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Upload_WithValidFile_ReturnsCreated()
    {
        // Arrange
        var dto = new DocumentDto { Id = Guid.NewGuid(), FileName = "doc.pdf", ContentType = "application/pdf" };

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.FileName).Returns("doc.pdf");

        _serviceMock.Setup(s => s.UploadDocumentAsync(It.IsAny<UploadDocumentRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dto);

        // Act
        var result = await _controller.Upload(fileMock.Object, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(dto, created.Value);
        Assert.Equal(nameof(DocumentsController.GetById), created.ActionName);
    }

    [Fact]
    public async Task Upload_WithNullFile_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Upload(null!, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Upload_WithEmptyFile_ReturnsBadRequest()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        // Act
        var result = await _controller.Upload(fileMock.Object, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_WhenFound_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteDocumentAsync(id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteDocumentAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Download_WhenFound_ReturnsFile()
    {
        // Arrange
        var id = Guid.NewGuid();
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        _serviceMock.Setup(s => s.DownloadDocumentAsync(id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((stream, "application/pdf", "file.pdf"));

        // Act
        var result = await _controller.Download(id, CancellationToken.None);

        // Assert
        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal("file.pdf", fileResult.FileDownloadName);
    }

    [Fact]
    public async Task Download_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.DownloadDocumentAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(((Stream Content, string ContentType, string FileName)?)null);

        // Act
        var result = await _controller.Download(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetAudit_WhenFound_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var audits = new List<DocumentAuditDto>
        {
            new() { Id = Guid.NewGuid(), DocumentId = id, Action = "Uploaded" }
        }.AsReadOnly();

        _serviceMock.Setup(s => s.GetDocumentAuditAsync(id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(audits);

        // Act
        var result = await _controller.GetAudit(id, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(audits, ok.Value);
    }

    [Fact]
    public async Task GetAudit_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetDocumentAuditAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IReadOnlyList<DocumentAuditDto>?)null);

        // Act
        var result = await _controller.GetAudit(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
