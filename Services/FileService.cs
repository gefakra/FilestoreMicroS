using FilestoreMicroS.Data;
using FilestoreMicroS.Models;
using FilestoreMicroS.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace FilestoreMicroS.Services
{
    public class FileService : IFileService
    {
        private readonly IFileProcessingService _processor;
        private readonly IStorageService _storage;
        private readonly IFileRepository _repo;
        private readonly StorageOptions _opts;

        public FileService(IFileProcessingService processor, IStorageService storage, IFileRepository repo, IOptions<StorageOptions> opts)
        {
            _processor = processor;
            _storage = storage;
            _repo = repo;
            _opts = opts.Value;
        }

        public async Task<string> StoreAsync(Stream input, Guid ownerId, CancellationToken ct = default)
        {
            // pipeline: process -> get hash + temp file -> move to final -> update metadata (with race handling)
            var (hash, tempPath, originalSize) = await _processor.ProcessAsync(input, ct);

            var finalRelative = await _storage.MoveTempToFinalAsync(tempPath, hash, ct);

            var exists = await _repo.FileExistsAsync(hash, ct);
            if (!exists)
            {
                var fileEntity = new FileEntity
                {
                    Hash = hash,
                    FilePath = finalRelative,
                    OriginalSize = originalSize,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _repo.AddFileWithOwnerAsync(fileEntity, ownerId, ct);
            }
            else
            {
                await _repo.AddOwnerIfMissingAsync(hash, ownerId, ct);
            }

            return hash;
        }

        public async Task<(Stream? Stream, long OriginalSize)> GetFileAsync(string hash, CancellationToken ct = default)
        {
            var file = await _repo.GetFileAsync(hash, ct);
            if (file == null) return (null, 0);

            var fullPath = Path.GetFullPath(file.FilePath);
            if (!File.Exists(fullPath)) return (null, 0);

            var fs = File.OpenRead(fullPath);
            var gz = new GZipStream(fs, CompressionMode.Decompress, leaveOpen: false);
            return (gz, file.OriginalSize);
        }

        public Task<bool> FileExistsAsync(string hash, CancellationToken ct = default) => _repo.FileExistsAsync(hash, ct);
        public Task<int> RemoveOwnerFromAllFilesAsync(Guid ownerId, CancellationToken ct = default) => _repo.RemoveOwnerFromAllFilesAsync(ownerId, ct);
        public Task<bool> RemoveOwnerFromFileAsync(string hash, Guid ownerId, CancellationToken ct = default) => _repo.RemoveOwnerFromFileAsync(hash, ownerId, ct);
    }
}
