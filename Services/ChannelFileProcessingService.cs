using FilestoreMicroS.Services.Interface;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FilestoreMicroS.Services
{
    public class StorageOptions
    {
        public string FilesPath { get; set; } = "data/files";
        public string TempPath { get; set; } = "data/tmp";
        public int BufferSize { get; set; } = 64 * 1024;
        public int ChannelCapacity { get; set; } = 8;
    }
    public class ChannelFileProcessingService : IFileProcessingService
    {
        private readonly StorageOptions _opts;

        public ChannelFileProcessingService(IOptions<StorageOptions> opts)
        {
            _opts = opts.Value;
            Directory.CreateDirectory(_opts.FilesPath);
            Directory.CreateDirectory(_opts.TempPath);
        }

        public async Task<(string Hash, string TempFilePath, long OriginalSize)> ProcessAsync(Stream input, CancellationToken ct = default)
        {
            var bufferSize = Math.Max(4096, _opts.BufferSize);
            var capacity = Math.Max(2, _opts.ChannelCapacity);

            var channel1 = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(capacity) { SingleReader = true, SingleWriter = true, FullMode = BoundedChannelFullMode.Wait });
            var channel2 = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(capacity) { SingleReader = true, SingleWriter = true, FullMode = BoundedChannelFullMode.Wait });

            var tempFile = Path.Combine(_opts.TempPath, $"filestore_{Guid.NewGuid():N}.tmp");

            long totalRead = 0;
            var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            // Producer: read input -> channel1
            var readerTask = Task.Run(async () =>
            {
                var pool = ArrayPool<byte>.Shared;
                var rented = pool.Rent(bufferSize);
                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        int read = await input.ReadAsync(rented, 0, bufferSize, ct).ConfigureAwait(false);
                        if (read == 0) break;
                        totalRead += read;
                        // copy to exact-sized array to avoid keeping oversized buffers in channel
                        var chunk = new byte[read];
                        Buffer.BlockCopy(rented, 0, chunk, 0, read);
                        await channel1.Writer.WriteAsync(chunk, ct).ConfigureAwait(false);
                    }
                }
                finally
                {
                    channel1.Writer.Complete();
                    pool.Return(rented);
                }
            }, ct);

            // Hasher: read channel1 -> update hash -> forward to channel2
            var hasherTask = Task.Run(async () =>
            {
                await foreach (var chunk in channel1.Reader.ReadAllAsync(ct))
                {
                    hasher.AppendData(chunk);
                    await channel2.Writer.WriteAsync(chunk, ct);
                }
                channel2.Writer.Complete();
            }, ct);

            // Compressor+Writer: read channel2 -> write gzip to temp file (single writer preserves order)
            var compressorTask = Task.Run(async () =>
            {
                await using var fs = new FileStream(tempFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous | FileOptions.WriteThrough);
                await using var gzip = new GZipStream(fs, CompressionLevel.Optimal, leaveOpen: true);

                await foreach (var chunk in channel2.Reader.ReadAllAsync(ct))
                {
                    await gzip.WriteAsync(chunk, 0, chunk.Length, ct).ConfigureAwait(false);
                }
                await gzip.FlushAsync(ct).ConfigureAwait(false);
                await fs.FlushAsync(ct).ConfigureAwait(false);
            }, ct);

            try
            {
                await Task.WhenAll(readerTask, hasherTask, compressorTask).ConfigureAwait(false);
            }
            catch
            {
                // cleanup temp on error
                try { if (File.Exists(tempFile)) File.Delete(tempFile); } catch { }
                throw;
            }

            var hashBytes = hasher.GetHashAndReset();
            var hashHex = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            return (hashHex, tempFile, totalRead);
        }
    }
}
