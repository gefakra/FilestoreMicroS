using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FilestoreMicroS.Services.Interface
{
    public interface IFileProcessingService
    {
        Task<(string Hash, string TempFilePath, long OriginalSize)> ProcessAsync(Stream input, CancellationToken ct = default);
    }
}
