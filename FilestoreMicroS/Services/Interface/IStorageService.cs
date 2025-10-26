using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FilestoreMicroS.Services.Interface
{
    public interface IStorageService
    {
        Task<string> MoveTempToFinalAsync(string tempFilePath, string hash, CancellationToken ct = default);
        Task<Stream?> OpenReadAsync(string relativePath, CancellationToken ct = default);
        Task<bool> ExistsAsync(string relativePath);
        Task DeleteAsync(string relativePath);
    }
}
