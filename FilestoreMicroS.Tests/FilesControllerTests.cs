using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FilestoreMicroS.Controllers;
using FilestoreMicroS.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace FilestoreMicroS.Tests
{
    public class FilesControllerTests
    {
        private FilesController CreateControllerWithMock(out Mock<IFileService> mockSvc)
        {
            mockSvc = new Mock<IFileService>();
            var ctrl = new FilesController(mockSvc.Object);
            // Смоканный HttpContext с пустым потоком
            var context = new DefaultHttpContext();
            context.Request.Body = new MemoryStream();
            ctrl.ControllerContext = new ControllerContext() { HttpContext = context };
            return ctrl;
        }

        [Fact]
        public async Task Upload_ShouldReturnOk_WithHash()
        {
            var ctrl = CreateControllerWithMock(out var mockSvc);

            var ownerId = Guid.NewGuid();
            mockSvc.Setup(s => s.StoreAsync(It.IsAny<Stream>(), ownerId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync("abc123");

            var result = await ctrl.Upload(ownerId, CancellationToken.None);

            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.Value.Should().BeEquivalentTo(new { sha256 = "abc123" });
        }

        [Fact]
        public async Task Download_ShouldReturnFile_WhenExists()
        {
            var ctrl = CreateControllerWithMock(out var mockSvc);

            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            mockSvc.Setup(s => s.GetFileAsync("hash1", It.IsAny<CancellationToken>()))
                   .ReturnsAsync((stream, 123L));

            var result = await ctrl.Download("hash1", CancellationToken.None);

            result.Should().BeOfType<FileStreamResult>();
            var fileResult = result as FileStreamResult;
            fileResult!.FileDownloadName.Should().BeNullOrEmpty(); // Download не задаёт имя
            fileResult.ContentType.Should().Be("application/octet-stream");
        }

        [Fact]
        public async Task Download_ShouldReturnNotFound_WhenMissing()
        {
            var ctrl = CreateControllerWithMock(out var mockSvc);

            mockSvc.Setup(s => s.GetFileAsync("hash1", It.IsAny<CancellationToken>()))
                   .ReturnsAsync((null, 0L));

            var result = await ctrl.Download("hash1", CancellationToken.None);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Exists_ShouldReturn204_WhenExists()
        {
            var ctrl = CreateControllerWithMock(out var mockSvc);

            mockSvc.Setup(s => s.FileExistsAsync("hash1", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(true);

            var result = await ctrl.Exists("hash1", CancellationToken.None);

            result.Should().BeOfType<StatusCodeResult>();
            (result as StatusCodeResult)!.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task Exists_ShouldReturnNotFound_WhenMissing()
        {
            var ctrl = CreateControllerWithMock(out var mockSvc);

            mockSvc.Setup(s => s.FileExistsAsync("hash1", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(false);

            var result = await ctrl.Exists("hash1", CancellationToken.None);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteOwnerFiles_ShouldReturnRemovedCount()
        {
            var ctrl = CreateControllerWithMock(out var mockSvc);

            mockSvc.Setup(s => s.RemoveOwnerFromAllFilesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(5);

            var ownerId = Guid.NewGuid();
            var result = await ctrl.DeleteOwnerFiles(ownerId, CancellationToken.None);

            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.Value.Should().BeEquivalentTo(new { removedReferences = 5 });
        }

        [Fact]
        public async Task DeleteOwnerFromFile_ShouldReturnNoContent_WhenRemoved()
        {
            var ctrl = CreateControllerWithMock(out var mockSvc);

            mockSvc.Setup(s => s.RemoveOwnerFromFileAsync("hash1", It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(true);

            var ownerId = Guid.NewGuid();
            var result = await ctrl.DeleteOwnerFromFile("hash1", ownerId, CancellationToken.None);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteOwnerFromFile_ShouldReturnNotFound_WhenMissing()
        {
            var ctrl = CreateControllerWithMock(out var mockSvc);

            mockSvc.Setup(s => s.RemoveOwnerFromFileAsync("hash1", It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(false);

            var ownerId = Guid.NewGuid();
            var result = await ctrl.DeleteOwnerFromFile("hash1", ownerId, CancellationToken.None);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
