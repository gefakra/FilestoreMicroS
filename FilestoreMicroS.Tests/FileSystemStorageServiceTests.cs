using System;
using System.IO;
using System.Threading.Tasks;
using FilestoreMicroS.Services;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Xunit;

namespace FilestoreMicroS.Tests
{
    public class FileSystemStorageServiceTests : IDisposable
    {
        private readonly string _baseDir;
        private readonly string _tempDir;
        private readonly string _finalDir;

        public FileSystemStorageServiceTests()
        {
            // Абсолютные пути относительно текущей рабочей директории теста
            _baseDir = Path.Combine(AppContext.BaseDirectory, "testdata");
            _tempDir = Path.Combine(_baseDir, "tmp");
            _finalDir = Path.Combine(_baseDir, "files");

            Directory.CreateDirectory(_tempDir);
            Directory.CreateDirectory(_finalDir);
        }

        [Fact]
        public async Task MoveTempToFinal_ShouldMoveFileAndReturnPath()
        {
            // Создаём временный файл
            var tempFile = Path.Combine(_tempDir, "temp.tmp");
            await using (var fs = File.Create(tempFile))
            {
                byte[] data = { 1, 2, 3 };
                await fs.WriteAsync(data);
            } // поток закрыт автоматически

            // Настройки для сервиса
            var opts = Options.Create(new StorageOptions { FilesPath = _finalDir });
            var storage = new FileSystemStorageService(opts);

            // Перемещаем временный файл в финальную папку
            var result = await storage.MoveTempToFinalAsync(tempFile, "abcd");

            // Проверяем, что файл существует
            File.Exists(result).Should().BeTrue();

            // Проверяем, что временный файл удалён
            File.Exists(tempFile).Should().BeFalse();
        }

        [Fact]
        public async Task MoveTempToFinal_ShouldDeleteTempIfFinalExists()
        {
            // Создаём финальный файл заранее
            var finalFile = Path.Combine(_finalDir, "abcd.gz");
            await File.WriteAllBytesAsync(finalFile, new byte[] { 9, 8, 7 });

            // Создаём временный файл
            var tempFile = Path.Combine(_tempDir, "temp2.tmp");
            await File.WriteAllBytesAsync(tempFile, new byte[] { 1, 2, 3 });

            var opts = Options.Create(new StorageOptions { FilesPath = _finalDir });
            var storage = new FileSystemStorageService(opts);

            var result = await storage.MoveTempToFinalAsync(tempFile, "abcd");

            // Файл в финальной директории должен остаться без изменений
            File.Exists(finalFile).Should().BeTrue();

            // Временный файл должен быть удалён
            File.Exists(tempFile).Should().BeFalse();
        }

        // Очистка после тестов
        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_baseDir))
                {
                    Directory.Delete(_baseDir, recursive: true);
                }
            }
            catch
            {
                // Игнорируем ошибки при удалении, например если файл заблокирован
            }
        }
    }
}
