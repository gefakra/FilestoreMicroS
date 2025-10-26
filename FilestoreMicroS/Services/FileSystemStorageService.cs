using FilestoreMicroS.Services.Interface;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FilestoreMicroS.Services
{
    public class FileSystemStorageService : IStorageService
    {
        private readonly string _baseDir;
        // constructor accepts StorageOptions via DI binding in Program.cs
        public FileSystemStorageService(IOptions<StorageOptions> opts)
        {
            _baseDir = opts.Value.FilesPath ?? "data/files";
            Directory.CreateDirectory(_baseDir);
        }

        public Task<string> MoveTempToFinalAsync(string tempFilePath, string hash, CancellationToken ct = default)
        {
            var finalName = $"{hash}.gz";
            var finalPath = Path.Combine(_baseDir, finalName);

            // If file already exists - delete temp and return path
            if (File.Exists(finalPath))
            {
                try { File.Delete(tempFilePath); } catch { }
                return Task.FromResult(Path.GetRelativePath(Directory.GetCurrentDirectory(), finalPath));
            }

            try
            {
                // atomic move when on same FS
                File.Move(tempFilePath, finalPath);
            }
            catch (IOException)
            {
                // fallback to copy
                using var src = File.OpenRead(tempFilePath);
                using var dst = File.Create(finalPath);
                src.CopyTo(dst);
                dst.Flush();
                try { File.Delete(tempFilePath); } catch { }
            }

            return Task.FromResult(Path.GetRelativePath(Directory.GetCurrentDirectory(), finalPath));
        }

        public Task<Stream?> OpenReadAsync(string relativePath, CancellationToken ct = default)
        {
            var full = Path.GetFullPath(relativePath);
            if (!File.Exists(full)) return Task.FromResult<Stream?>(null);
            var fs = File.OpenRead(full);
            return Task.FromResult<Stream?>(fs);
        }

        public Task<bool> ExistsAsync(string relativePath)
        {
            return Task.FromResult(File.Exists(Path.GetFullPath(relativePath)));
        }

        public Task DeleteAsync(string relativePath)
        {
            var full = Path.GetFullPath(relativePath);
            if (File.Exists(full)) File.Delete(full);
            return Task.CompletedTask;
        }
    }
}
