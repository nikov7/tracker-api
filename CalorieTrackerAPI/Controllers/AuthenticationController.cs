using Microsoft.AspNetCore.Mvc;
using CalorieTrackerAPI.Models;
using CalorieTrackerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;


namespace CalorieTrackerAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationService _authService;
        private readonly ApplicationContext _context;

        public AuthenticationController(ApplicationContext context, AuthenticationService authService)
        {
            _context = context;
            _authService = authService;
        }

        [Authorize]
        [HttpGet("verify")]
        public async Task<ActionResult<UserDto>> Verify()
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (tokenUserId == null)
            {
                return Unauthorized();
            }

            var tokenProfileId = User.FindFirst("profileId")?.Value;
            long userId = Convert.ToInt64(tokenUserId);

            var existingUser = await _context.Users
                .Include(user => user.Profiles) // Eagerly load profiles
                .Where(user => user.Id == userId)
                .FirstOrDefaultAsync();


            if (existingUser == null)
            {
                return NotFound();
            }

            return Ok(new { userId = tokenUserId, profileId = tokenProfileId });
        }
        
        [Authorize]
        [HttpPost("switch")]
        public async Task<IActionResult> Switch([FromBody] ProfileSwitchRequest request)
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tokenProfileId = User.FindFirst("profileId")?.Value;
            
            if (tokenUserId == null) // add another check for profileid
            {
                return Unauthorized();
            }
            
            long userId = Convert.ToInt64(tokenUserId);
            long profileId = Convert.ToInt64(tokenProfileId);

            var existingProfile = await _context.Profiles
                .Where(p => p.UserId == userId && p.Id == request.ProfileId)
                .FirstOrDefaultAsync();

            if (existingProfile == null)
            {
                return NotFound();
            }

            if (profileId == request.ProfileId)
            {
                return BadRequest();
            }
            
            
            var token = _authService.GenerateJwtToken(userId, request.ProfileId);
            
            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Expires = DateTime.Now.AddMinutes(60), // 60 minutes
            });
            
            
            return Ok(new { Message = "Switched successfully" });
        }
        
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginRequest request)
        {
            var existingUser = await _context.Users
                .Include(user=> user.Profiles) // Eagerly load profiles
                .Where(user => user.Name == request.Name)
                .FirstOrDefaultAsync();

            if (existingUser == null)
            {
                return NotFound();
            }
            
            // User was found
            var defaultProfile = await _context.Profiles
                .Where(p => p.UserId == existingUser.Id)
                .FirstOrDefaultAsync();
            
            if (defaultProfile == null) // search for profile
            {
                Console.WriteLine("Couldn't find profile id");
                return NotFound();
            }

            var token = _authService.GenerateJwtToken(existingUser.Id, defaultProfile.Id);
            
            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Expires = DateTime.Now.AddMinutes(60),
            });

            var userDto = new UserDto
            {
                Id = existingUser.Id,
                Name = existingUser.Name,
                Profiles = existingUser.Profiles!.Select(p => new ProfileDto
                {
                    Id = p.Id,
                    Name = p.Name
                }).ToList()
            };
            return userDto;
        }
        
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // modify to check if user even is logged in
            Response.Cookies.Delete("access_token");
            return Ok(new { Message = "Logged out successfully" });
        }

    }
    

    public class LoginRequest
    {
        public String Name { get; set; }
    }

    public class ProfileSwitchRequest
    {
        public long ProfileId { get; set; }
    }
}