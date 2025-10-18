using FacesmashAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacesmashAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceController : ControllerBase
    {
        private readonly AppDbContext _db;
        public MaintenanceController(AppDbContext db)
        {
            _db = db;
        }

        // POST api/maintenance/fix-photo-urls
        [HttpPost("fix-photo-urls")]
        public async Task<IActionResult> FixPhotoUrls()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
            var users = await _db.Users.Where(u => u.PhotoUrl != null && u.PhotoUrl.StartsWith("/uploads/")).ToListAsync();

            foreach (var user in users)
            {
                user.PhotoUrl = baseUrl + user.PhotoUrl;
            }

            await _db.SaveChangesAsync();
            return Ok(new { updated = users.Count, baseUrl });
        }
    }
}


