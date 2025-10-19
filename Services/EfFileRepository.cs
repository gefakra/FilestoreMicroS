using FilestoreMicroS.Data;
using FilestoreMicroS.Models;
using FilestoreMicroS.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FilestoreMicroS.Services
{
    public class EfFileRepository : IFileRepository
    {
        private readonly FileStoreContext _db;
        public EfFileRepository(FileStoreContext db) => _db = db;

        public async Task<FileEntity?> GetFileAsync(string hash, CancellationToken ct = default)
            => await _db.Files.Include(f => f.Owners).FirstOrDefaultAsync(f => f.Hash == hash, ct);

        public async Task AddFileWithOwnerAsync(FileEntity file, Guid ownerId, CancellationToken ct = default)
        {
            // attach/create owner
            var owner = await _db.Owners.FindAsync(new object[] { ownerId }, ct) ?? new Owner { Id = ownerId };
            if (_db.Entry(owner).State == EntityState.Detached) _db.Owners.Attach(owner);

            file.Owners = file.Owners ?? new List<FileOwner>();
            file.Owners.Add(new FileOwner { OwnerId = ownerId, FileHash = file.Hash, Owner = owner });

            _db.Files.Add(file);
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                // race: someone else inserted same file concurrently -> ignore; caller should add owner if needed
            }
        }

        public async Task<bool> AddOwnerIfMissingAsync(string hash, Guid ownerId, CancellationToken ct = default)
        {
            var file = await _db.Files.Include(f => f.Owners).FirstOrDefaultAsync(f => f.Hash == hash, ct);
            if (file == null) return false;
            if (file.Owners.Any(o => o.OwnerId == ownerId)) return true;

            var owner = await _db.Owners.FindAsync(new object[] { ownerId }, ct) ?? new Owner { Id = ownerId };
            if (_db.Entry(owner).State == EntityState.Detached) _db.Owners.Attach(owner);

            file.Owners.Add(new FileOwner { OwnerId = ownerId, FileHash = file.Hash, Owner = owner });
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> RemoveOwnerFromAllFilesAsync(Guid ownerId, CancellationToken ct = default)
        {
            var owners = await _db.FileOwners.Where(fo => fo.OwnerId == ownerId).ToListAsync(ct);
            if (!owners.Any()) return 0;
            _db.FileOwners.RemoveRange(owners);
            await _db.SaveChangesAsync(ct);

            var orphaned = await _db.Files.Include(f => f.Owners).Where(f => !f.Owners.Any()).ToListAsync(ct);
            if (orphaned.Any())
            {
                _db.Files.RemoveRange(orphaned);
                await _db.SaveChangesAsync(ct);
            }

            var ownerEntity = await _db.Owners.FindAsync(new object[] { ownerId }, ct);
            if (ownerEntity != null)
            {
                _db.Owners.Remove(ownerEntity);
                await _db.SaveChangesAsync(ct);
            }

            return owners.Count;
        }

        public async Task<bool> RemoveOwnerFromFileAsync(string hash, Guid ownerId, CancellationToken ct = default)
        {
            var fo = await _db.FileOwners.FirstOrDefaultAsync(x => x.FileHash == hash && x.OwnerId == ownerId, ct);
            if (fo == null) return false;
            _db.FileOwners.Remove(fo);
            await _db.SaveChangesAsync(ct);

            var file = await _db.Files.Include(f => f.Owners).FirstOrDefaultAsync(f => f.Hash == hash, ct);
            if (file != null && !file.Owners.Any())
            {
                _db.Files.Remove(file);
                await _db.SaveChangesAsync(ct);
            }

            var owner = await _db.Owners.Include(o => o.FileOwners).FirstOrDefaultAsync(o => o.Id == ownerId, ct);
            if (owner != null && !owner.FileOwners.Any())
            {
                _db.Owners.Remove(owner);
                await _db.SaveChangesAsync(ct);
            }

            return true;
        }

        public async Task<bool> FileExistsAsync(string hash, CancellationToken ct = default)
            => await _db.Files.AnyAsync(f => f.Hash == hash, ct);
    }
}
