using System;
using System.Threading.Tasks;
using FilestoreMicroS.Data;
using FilestoreMicroS.Models;
using FilestoreMicroS.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace FilestoreMicroS.Tests
{
    public class EfFileRepositoryTests
    {
        private FileStoreContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<FileStoreContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new FileStoreContext(options);
        }

        [Fact]
        public async Task AddFileWithOwner_ShouldAddEntity()
        {
            using var ctx = CreateContext();
            var repo = new EfFileRepository(ctx);

            var file = new FileEntity { Hash = "abc", FilePath = "data/test.gz", OriginalSize = 100 };
            var owner = Guid.NewGuid();

            await repo.AddFileWithOwnerAsync(file, owner);
            var found = await ctx.Files.Include(f => f.Owners).FirstOrDefaultAsync(f => f.Hash == "abc");

            found.Should().NotBeNull();
            found!.Owners.Should().Contain(o => o.OwnerId == owner);
        }
    }
}
