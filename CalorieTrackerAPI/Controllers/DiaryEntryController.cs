using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CalorieTrackerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace CalorieTrackerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DiaryEntryController : ControllerBase
    {
        private readonly ApplicationContext _context;

        private readonly ILogger<DiaryEntryController> _logger;

        public DiaryEntryController(ApplicationContext context, ILogger<DiaryEntryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // its better not to expose userId, let backend know userId from JWT token Sub
        [HttpGet("{dateString}")]
        public async Task<ActionResult<IEnumerable<FoodIntakeDto>>> GetDiaryEntry(string dateString)
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var tokenProfileId = User.FindFirst("profileId")?.Value;
            
            if (tokenUserId == null) // add another check for profileid
            {
                return Unauthorized();
            }

            long userId = Convert.ToInt64(tokenUserId);
            long profileId = Convert.ToInt64(tokenProfileId);


            DateOnly date = DateOnly.Parse(dateString);

            var diaryEntry = await _context.DiaryEntries
                .Where(diary => diary.ProfileId == profileId && diary.Date == date)
                .FirstOrDefaultAsync();
            
            if (diaryEntry == null)
            {
                return new List<FoodIntakeDto>();
            }

            var foodIntakesQuery = _context.FoodIntakes
                .Include(fi => fi.FoodItem)
                .ThenInclude(f => f!.ServingSizes)
                .Include(fi => fi.ServingSize)
                .Where(fi => fi.DiaryEntryId == diaryEntry.Id)
                .Select(fi => new
                {
                    FoodIntake = fi,
                    ServingSizes = fi.FoodItem!.ServingSizes!.Select(ss => new ServingSizeDto
                    {
                        Id = ss.Id,
                        Name = ss.Name,
                        Amount = ss.Amount
                    }).ToList()
                });

            var foodIntakes = new List<FoodIntakeDto>();

            foreach (var fi in await foodIntakesQuery.ToListAsync())
            {
                fi.ServingSizes.Add(new ServingSizeDto
                {
                    Id = 0,
                    Name = "g",
                    Amount = 1
                });

                var servingSizeId = fi.FoodIntake.ServingSizeId ?? 0;

                foodIntakes.Add(new FoodIntakeDto
                {
                    Id = fi.FoodIntake.Id,
                    Name = fi.FoodIntake.FoodItem!.Name,
                    FoodItemId = fi.FoodIntake.FoodItemId,
                    Energy = fi.FoodIntake.FoodItem!.Energy,
                    Carbs = fi.FoodIntake.FoodItem!.Carbs,
                    Fat = fi.FoodIntake.FoodItem!.Fat,
                    Protein = fi.FoodIntake.FoodItem!.Protein,
                    Quantity = fi.FoodIntake.Quantity,
                    ServingSizeIndex = fi.ServingSizes.FindIndex(ss => ss.Id == servingSizeId),
                    AvailableServingSizes = fi.ServingSizes
                });
            }
            return foodIntakes;
        }
        
        [HttpPost("{dateString}")]
        public async Task<ActionResult<DiaryEntry>> AddFoodIntake(string dateString, FoodIntake foodIntake)
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tokenProfileId = User.FindFirst("profileId")?.Value;

            if (tokenUserId == null)
            {
                return Unauthorized();
            }
            
            long userId = Convert.ToInt64(tokenUserId);
            long profileId = Convert.ToInt64(tokenProfileId);

            var existingFoodItem = await _context.FoodItems
                .Where(fi => fi.Id == foodIntake.FoodItemId && fi.UserId == userId)
                .FirstOrDefaultAsync();

            if (existingFoodItem == null)
            {
                return NotFound();
            }
            
            
            //change this function's name to something else
            DateOnly date = DateOnly.Parse(dateString);

            var diaryEntry = await _context.DiaryEntries
                .Where(diary => diary.ProfileId == profileId && diary.Date == date)
                .FirstOrDefaultAsync();

            if (diaryEntry == null)
            {
                Console.WriteLine("No diary exists, creating one...");
                diaryEntry = new DiaryEntry
                {
                    ProfileId = profileId,
                    Date = date
                };
                _context.DiaryEntries.Add(diaryEntry);
                await _context.SaveChangesAsync();
            }
            
            foodIntake.DiaryEntryId = diaryEntry.Id;

            if (foodIntake.ServingSizeId == 0)
            {
                Console.WriteLine("Serving Size ID was 0, setting to null");
                foodIntake.ServingSizeId = null;
            }
            
            
            
            // will need to specify serving size id
            // var existingServingSize = await _context.ServingSizes
            //     .Where(ss=> ss.FoodItemId == foodIntake.FoodItemId)
            
            _context.FoodIntakes.Add(foodIntake);
            await _context.SaveChangesAsync();
            
            // change routeValues later
            return CreatedAtAction(nameof(GetDiaryEntry), new { dateString = date.ToString() }, foodIntake);
        }
        
        [HttpPut("food/{id}")]
        public async Task<IActionResult> UpdateFoodIntake(long id, FoodIntake foodIntake)
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tokenProfileId = User.FindFirst("profileId")?.Value;

            if (tokenUserId == null)
            {
                return Unauthorized();
            }
            
            long userId = Convert.ToInt64(tokenUserId);
            long profileId = Convert.ToInt64(tokenProfileId);
            
            if (id != foodIntake.Id)
            {
                return BadRequest();
            }
            // do check its only being accessed by the right userid (now profileid)
            var existingFoodIntake = await _context.FoodIntakes
                .Include(fi=> fi.DiaryEntry)
                .Where(fi => fi.Id == id)
                .FirstOrDefaultAsync();
            
            
            // specify serving size ID, make sure serving size ID belongs to him

            if (existingFoodIntake == null || existingFoodIntake.DiaryEntry!.ProfileId != profileId)
            {
                return NotFound();
            }

            foodIntake.DiaryEntryId = existingFoodIntake.DiaryEntryId;
            foodIntake.FoodItemId = existingFoodIntake.FoodItemId;

            if (foodIntake.ServingSizeId == null)
            {
                Console.WriteLine("Serving Size ID not specified, setting it to previous");
                foodIntake.ServingSizeId = existingFoodIntake.ServingSizeId;
            }
            
            if (foodIntake.ServingSizeId == 0)
            {
                Console.WriteLine("Serving Size ID was 0, setting to null");
                foodIntake.ServingSizeId = null;
            }
            

            if (foodIntake.Quantity == null)
            {
                Console.WriteLine("Quantity not specified, setting to previous");
                foodIntake.Quantity = existingFoodIntake.Quantity;
            }

            
            
            _context.Entry(existingFoodIntake).CurrentValues.SetValues(foodIntake);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        
        // DELETE: api/DiaryEntry/5
        [HttpDelete("food/{id}")]
        public async Task<IActionResult> DeleteFoodIntake(long id)
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tokenProfileId = User.FindFirst("profileId")?.Value;

            if (tokenUserId == null)
            {
                return Unauthorized();
            }
            
            long userId = Convert.ToInt64(tokenUserId);
            long profileId = Convert.ToInt64(tokenProfileId);


            var foodIntake = await _context.FoodIntakes
                .Include(fi => fi.DiaryEntry)
                .Where(fi => fi.Id == id)
                .FirstOrDefaultAsync();
            
            if (foodIntake == null || foodIntake.DiaryEntry!.ProfileId != profileId)
            {
                return NotFound();
            }

            _context.FoodIntakes.Remove(foodIntake);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

