using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilestoreMicroS.Models
{
    public class FileEntity
    {
        [Key]        
        [MaxLength(64)]
        public string Hash { get; set; } = null!; // SHA256 hex

        [Required]
        public string FilePath { get; set; } = null!; // relative path to compressed file
        public long OriginalSize { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public List<FileOwner> Owners { get; set; } = new();
    }
}
