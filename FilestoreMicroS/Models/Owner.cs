using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FilestoreMicroS.Models
{
    public class Owner
    {
        [Key]
        public Guid Id { get; set; }
        public List<FileOwner> FileOwners { get; set; } = new();
    }
}
