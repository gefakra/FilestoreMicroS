using FilestoreMicroS.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FilestoreMicroS.Services.Interface
{
    public interface IFileRepository
    {
        Task<FileEntity?> GetFileAsync(string hash, CancellationToken ct = default);
        Task AddFileWithOwnerAsync(FileEntity file, Guid ownerId, CancellationToken ct = default);
        Task<bool> AddOwnerIfMissingAsync(string hash, Guid ownerId, CancellationToken ct = default);
        Task<int> RemoveOwnerFromAllFilesAsync(Guid ownerId, CancellationToken ct = default);
        Task<bool> RemoveOwnerFromFileAsync(string hash, Guid ownerId, CancellationToken ct = default);
        Task<bool> FileExistsAsync(string hash, CancellationToken ct = default);
    }
}
