using System;
using System.IO;
using System.Threading.Tasks;
using FilestoreMicroS.Services;
using Microsoft.Extensions.Options;
using Xunit;
using FluentAssertions;

namespace FilestoreMicroS.Tests
{
    public class ChannelFileProcessingServiceTests
    {
        [Fact]
        public async Task ProcessAsync_ShouldProduceHashAndCompressedFile()
        {
            var opts = Options.Create(new StorageOptions
            {
                FilesPath = "data/files",
                TempPath = "data/tmp",
                BufferSize = 8192,
                ChannelCapacity = 4
            });

            Directory.CreateDirectory("data/tmp");
            Directory.CreateDirectory("data/files");

            var svc = new ChannelFileProcessingService(opts);
            var random = new byte[1024 * 128];
            new Random().NextBytes(random);

            using var stream = new MemoryStream(random);
            var (hash, tempFile, size) = await svc.ProcessAsync(stream);

            hash.Should().HaveLength(64);
            File.Exists(tempFile).Should().BeTrue();
            size.Should().Be(random.Length);

            Directory.Delete("data", true);
        }
    }
}
