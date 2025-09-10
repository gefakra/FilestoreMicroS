using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileStoreService.Models
{
    public class StoredFile
    {
        [Key]
        public int Id { get; set; }

        // SHA256 hex lowercase
        [Required]
        [MaxLength(64)]
        public string Hash { get; set; }

        [Required]
        public long OriginalSize { get; set; }

        // размер сжатого файла на диске
        public long CompressedSize { get; set; }

        public string ContentType { get; set; }

        // относительный путь к файлу (например: aa/bb/{hash}.gz)
        public string RelativePath { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<FileOwner> Owners { get; set; } = new List<FileOwner>();
    }

    public class Owner
    {
        [Key]
        public Guid Id { get; set; }

        public ICollection<FileOwner> Files { get; set; } = new List<FileOwner>();
    }

    public class FileOwner
    {
        public int StoredFileId { get; set; }
        public StoredFile StoredFile { get; set; }

        public Guid OwnerId { get; set; }
        public Owner Owner { get; set; }
    }
}
