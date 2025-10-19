using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilestoreMicroS.Models
{
    public class FileOwner
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string FileHash { get; set; } = null!;
        public FileEntity File { get; set; } = null!;

        [Required]
        public Guid OwnerId { get; set; }
        public Owner Owner { get; set; } = null!;
        public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
