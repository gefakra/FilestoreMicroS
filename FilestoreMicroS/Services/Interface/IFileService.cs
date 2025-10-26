using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FilestoreMicroS.Services.Interface
{
    public interface IFileService
    {
        Task<string> StoreAsync(Stream input, Guid ownerId, CancellationToken ct = default);
        Task<(Stream? Stream, long OriginalSize)> GetFileAsync(string hash, CancellationToken ct = default);
        Task<bool> FileExistsAsync(string hash, CancellationToken ct = default);
        Task<int> RemoveOwnerFromAllFilesAsync(Guid ownerId, CancellationToken ct = default);
        Task<bool> RemoveOwnerFromFileAsync(string hash, Guid ownerId, CancellationToken ct = default);
    }
}
