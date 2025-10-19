using FilestoreMicroS.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
namespace FilestoreMicroS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _svc;
        public FilesController(IFileService svc) => _svc = svc;

        // POST /api/files?owner={guid}
        [HttpPost]
        public async Task<IActionResult> Upload([FromQuery] Guid owner, CancellationToken ct)
        {
            if (owner == Guid.Empty) return BadRequest("owner query parameter must be a GUID");

            Stream stream;
            if (Request.HasFormContentType && Request.Form.Files.Count > 0)
            {
                stream = Request.Form.Files[0].OpenReadStream();
            }
            else
            {
                stream = Request.Body;
            }

            // Use RequestAborted token (ct passed from route)
            var hash = await _svc.StoreAsync(stream, owner, ct);
            return Ok(new { sha256 = hash });
        }

        // GET /api/files/{hash}
        [HttpGet("{hash}")]
        public async Task<IActionResult> Download([FromRoute] string hash, CancellationToken ct)
        {
            var (stream, size) = await _svc.GetFileAsync(hash, ct);
            if (stream == null) return NotFound();
            // stream is decompressed readable stream (GZipStream)
            return File(stream, "application/octet-stream", enableRangeProcessing: false);
        }

        // GET /api/files/{hash}/exists
        [HttpGet("{hash}/exists")]
        public async Task<IActionResult> Exists([FromRoute] string hash, CancellationToken ct)
        {
            var exists = await _svc.FileExistsAsync(hash, ct);
            return exists ? StatusCode(204) : NotFound();
        }

        // DELETE owner/{ownerId}
        [HttpDelete("owner/{ownerId}")]
        public async Task<IActionResult> DeleteOwnerFiles([FromRoute] Guid ownerId, CancellationToken ct)
        {
            var removed = await _svc.RemoveOwnerFromAllFilesAsync(ownerId, ct);
            return Ok(new { removedReferences = removed });
        }

        // DELETE {hash}/owner/{ownerId}
        [HttpDelete("{hash}/owner/{ownerId}")]
        public async Task<IActionResult> DeleteOwnerFromFile([FromRoute] string hash, [FromRoute] Guid ownerId, CancellationToken ct)
        {
            var ok = await _svc.RemoveOwnerFromFileAsync(hash, ownerId, ct);
            if (!ok) return NotFound();
            return NoContent();
        }
    }

}
