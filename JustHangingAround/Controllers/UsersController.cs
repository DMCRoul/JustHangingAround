using JustHangingAround.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JustHangingAround.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var currentUser = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(currentUser))
            {
                return Unauthorized();
            }

            var users = await _context.Users
                .Where(u => u.Username != currentUser)
                .OrderBy(u => u.Username)
                .Select(u => u.Username)
                .ToListAsync();

            return Ok(users);
        }
    }
}