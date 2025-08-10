using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftManagement.Services;

namespace ShiftManagement.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/migrate")]
    [Authorize(Roles = "Admin")]
    public class MigrationController : ControllerBase
    {
        private readonly ISamsDataMigrator _migrator;

        public MigrationController(ISamsDataMigrator migrator)
        {
            _migrator = migrator;
        }

        [HttpPost("sams")]
        public async Task<IActionResult> MigrateSams([FromQuery] bool overwrite = false, CancellationToken ct = default)
        {
            await _migrator.RunAsync(overwrite, ct);
            return Ok(new { message = "SAMS migration completed", overwrite });
        }
    }
}