using System;
using System.IO;
using System.Threading.Tasks;
using FilestoreMicroS.Data;
using FilestoreMicroS.Models;
using FilestoreMicroS.Services;
using FilestoreMicroS.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using FluentAssertions;

namespace FilestoreMicroS.Tests
{
    public class FileServiceTests
    {
        private FileStoreContext CreateContext()
        {
            var opts = new DbContextOptionsBuilder<FileStoreContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new FileStoreContext(opts);
        }

        [Fact]
        public async Task StoreAsync_ShouldSaveFileAndOwner()
        {
            using var ctx = CreateContext();
            var repo = new EfFileRepository(ctx);

            var mockProc = new Mock<IFileProcessingService>();
            var mockStorage = new Mock<IStorageService>();

            mockProc.Setup(p => p.ProcessAsync(It.IsAny<Stream>(), default))
                .ReturnsAsync(("abc123", "data/tmp/temp.gz", 1000));

            mockStorage.Setup(s => s.MoveTempToFinalAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .ReturnsAsync("data/files/abc123.gz");

            var opts = Options.Create(new StorageOptions());
            var svc = new FileService(mockProc.Object, mockStorage.Object, repo, opts);

            var owner = Guid.NewGuid();
            using var file = new MemoryStream(new byte[256]);
            var hash = await svc.StoreAsync(file, owner);

            hash.Should().Be("abc123");
            (await ctx.Files.AnyAsync(f => f.Hash == "abc123")).Should().BeTrue();
        }
    }
}
