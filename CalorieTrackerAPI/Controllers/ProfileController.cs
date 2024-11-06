
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CalorieTrackerAPI.Models;

namespace CalorieTrackerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        // make sure to do check to prevent deletion of DEFAULT profile. OR maybe prevent deletion if only 1 profile remains
        // also if deleting a profile you can't be using the profile
        private readonly ApplicationContext _context;

        public ProfileController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Profile
        [HttpGet]
        public async Task<ActionResult<ProfileInformation>> GetProfiles()
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tokenProfileId = User.FindFirst("profileId")?.Value;
            
            if (tokenUserId == null) // add another check for profileid
            {
                return Unauthorized();
            }
            long userId = Convert.ToInt64(tokenUserId);
            long profileId = Convert.ToInt64(tokenProfileId);
            

            var existingProfiles = await _context.Profiles
                .Where(p => p.UserId == userId)
                .ToListAsync();


            var profileIndex = existingProfiles.FindIndex(p=> p.Id == profileId);

            var profileInformation = new ProfileInformation
            {
                CurrentProfileIndex = profileIndex,
                Profiles = existingProfiles
            };
            

            return profileInformation;
        }

        // GET: api/Profile/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Profile>> GetProfile(long id)
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
                .Where(p => p.UserId == userId && p.Id == profileId)
                .FirstOrDefaultAsync();
            

            if (existingProfile == null)
            {
                return NotFound();
            }

            return existingProfile;
        }

        // PUT: api/Profile/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProfile(long id, Profile profile)
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tokenProfileId = User.FindFirst("profileId")?.Value;
            if (tokenUserId == null) // add another check for profileid
            {
                return Unauthorized();
            }
            long userId = Convert.ToInt64(tokenUserId);
            long profileId = Convert.ToInt64(tokenProfileId);
            
            if (id != profile.Id)
            {
                return BadRequest();
            }

            var existingProfile = await _context.Profiles
                .Where(p => p.UserId == userId && p.Id == profile.Id)
                .FirstOrDefaultAsync();

            if (existingProfile == null)
            {
                return NotFound();
            }

            profile.UserId = existingProfile.UserId;
            
            
            _context.Entry(existingProfile).CurrentValues.SetValues(profile);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfileExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Profile
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Profile>> PostProfile(Profile profile)
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tokenProfileId = User.FindFirst("profileId")?.Value;
            if (tokenUserId == null) // add another check for profileid
            {
                return Unauthorized();
            }
            long userId = Convert.ToInt64(tokenUserId);
            long profileId = Convert.ToInt64(tokenProfileId);
            
            profile.UserId = userId;
            

            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();
            
            Preference preference = new Preference
            {
                ProfileId = profile.Id
            };

            _context.Preferences.Add(preference);
            await _context.SaveChangesAsync();
            

            return CreatedAtAction(nameof(GetProfile), new { id = profile.Id }, profile);
        }

        // DELETE: api/Profile/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfile(long id)
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
                .Where(p => p.UserId == userId && p.Id == id)
                .FirstOrDefaultAsync();

            if (existingProfile == null)
            {
                return NotFound();
            }
            

            _context.Profiles.Remove(existingProfile);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        
        [HttpGet("preferences")]
        public async Task<ActionResult<PreferenceDto>> GetPreferences()
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tokenProfileId = User.FindFirst("profileId")?.Value;
            
            if (tokenUserId == null) // add another check for profileid
            {
                return Unauthorized();
            }
            long userId = Convert.ToInt64(tokenUserId);
            long profileId = Convert.ToInt64(tokenProfileId);
            

            var existingPreferences = await _context.Preferences
                .Where(pref => pref.ProfileId == profileId)
                .FirstOrDefaultAsync();

            if (existingPreferences == null)
            {
                return NotFound();
            }

            var preferenceDto = new PreferenceDto
            {
                Energy = existingPreferences.Energy,
                Carbs = existingPreferences.Carbs,
                Fat = existingPreferences.Fat,
                Protein = existingPreferences.Protein
            };
            
            return preferenceDto;
        }
        
        [HttpPut("preferences")]
        public async Task<ActionResult<Preference>> UpdatePreferences(Preference preference)
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tokenProfileId = User.FindFirst("profileId")?.Value;
            
            if (tokenUserId == null) // add another check for profileid
            {
                return Unauthorized();
            }
            long userId = Convert.ToInt64(tokenUserId);
            long profileId = Convert.ToInt64(tokenProfileId);
            

            var existingPreferences = await _context.Preferences
                .Where(pref => pref.ProfileId == profileId)
                .FirstOrDefaultAsync();

            if (existingPreferences == null)
            {
                return NotFound();
            }

            preference.Id = existingPreferences.Id;
            preference.ProfileId = existingPreferences.ProfileId;
            
            _context.Entry(existingPreferences).CurrentValues.SetValues(preference);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool ProfileExists(long id)
        {
            return _context.Profiles.Any(e => e.Id == id);
        }
    }


    public class ProfileInformation
    {
        public long CurrentProfileIndex { get; set; }
        public IEnumerable<Profile> Profiles { get; set; }
    }
}
