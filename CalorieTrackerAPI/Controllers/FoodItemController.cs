using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CalorieTrackerAPI.Models;
using Microsoft.AspNetCore.Authorization;


namespace CalorieTrackerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FoodItemController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public FoodItemController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/FoodItem
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FoodItemDto>>> GetFoodItems()
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (tokenUserId == null) // failed
            {
                return Unauthorized();
            }

            long userId = Convert.ToInt64(tokenUserId);

            var foodItems = await _context.FoodItems
                .Include(fi=> fi.ServingSizes)
                .Where(fi => fi.UserId == userId)
                .Select(fi => new FoodItemDto()
                {
                    Id = fi.Id,
                    Name = fi.Name,
                    Energy = fi.Energy,
                    Carbs = fi.Carbs,
                    Fat = fi.Fat,
                    Protein = fi.Protein,
                    ServingSizes = fi.ServingSizes!.Select(ss=> new ServingSizeDto
                    {
                        Id = ss.Id,
                        Name = ss.Name,
                        Amount = ss.Amount
                    }).ToList()
                })
                .ToListAsync();
            
            // Add the extra ServingSizeDto to each FoodItemDto
            foreach (var item in foodItems)
            {
                item.ServingSizes!.Add(new ServingSizeDto
                {
                    Id = 0,
                    Name = "g",
                    Amount = 1
                });
            }
            
            return foodItems;
        }

        // GET: api/FoodItem/1/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FoodItemDto>> GetFoodItem(long id)
        {
            // var foodItem = await _context.FoodItems.FindAsync(id);
            // Extract user ID from the JWT token

            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (tokenUserId == null)
            {
                return Unauthorized();
            }
            
            long userId = Convert.ToInt64(tokenUserId);
            

            var foodItem = await _context.FoodItems
                .Include(fi=> fi.ServingSizes)
                .Where(fi => fi.UserId == userId && fi.Id == id)
                .Select(fi => new FoodItemDto()
                {
                    Id = fi.Id,
                    Name = fi.Name,
                    Energy = fi.Energy,
                    Carbs = fi.Carbs,
                    Fat = fi.Fat,
                    Protein = fi.Protein,
                    ServingSizes = fi.ServingSizes!.Select(ss=> new ServingSizeDto
                    {
                        Id = ss.Id,
                        Name = ss.Name,
                        Amount = ss.Amount
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (foodItem == null)
            {
                return NotFound();
            }

            return foodItem;
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFoodItem(long id, FoodItem foodItem)
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (tokenUserId == null)
            {
                return Unauthorized();
            }
            
            long userId = Convert.ToInt64(tokenUserId);
            
            
            if (id != foodItem.Id)
            {
                return BadRequest();
            }
            
            var existingFoodItem = await _context.FoodItems
                .Where(fi => fi.Id == id && fi.UserId == userId)
                .FirstOrDefaultAsync();

            if (existingFoodItem == null)
            {
                return NotFound();
            }


            foodItem.UserId = userId;
            
            _context.Entry(existingFoodItem).CurrentValues.SetValues(foodItem);
            await _context.SaveChangesAsync();
            
            
            var existingServingSizes = await _context.ServingSizes
                .Where(ss => ss.FoodItemId == foodItem.Id)
                .ToListAsync();
            
            var servingSizeMap = existingServingSizes.ToDictionary(ss => ss.Id, ss => ss);
            
            
            

            foreach (ServingSize receivedServing in foodItem.ServingSizes!)
            {
                Console.WriteLine("Receiving: " + receivedServing.Id);
                if (servingSizeMap.TryGetValue(receivedServing.Id, out var existingServing))
                {
                    // Update existing serving size
                    Console.WriteLine("[UPDATING] ID: "+existingServing.Id + " Name: " + existingServing.Name + " Amount: " + existingServing.Amount + " Fooditemid: " + existingServing.FoodItemId);
                    existingServing.Name = receivedServing.Name;
                    existingServing.Amount = receivedServing.Amount;
                    
                    // Explicitly update the entity in the context
                    _context.ServingSizes.Update(existingServing);
                }
                else
                {
                    // Add new serving size
                    var newServingSize = new ServingSize
                    {
                        // Assign values from receivedServing
                        Name = receivedServing.Name,
                        Amount = receivedServing.Amount,
                        FoodItemId = foodItem.Id // Make sure to link it to the parent food item
                    };
                    Console.WriteLine("[ADDED NEW] ID: " + newServingSize.Id + " Name: " + newServingSize.Name + " Amount: " + newServingSize.Amount);
                    _context.ServingSizes.Add(newServingSize);
                }
            }
            
            var idsOfReceivedServings = foodItem.ServingSizes.Select(ss => ss.Id).ToHashSet();
            var servingSizesToDelete = existingServingSizes.Where(ss => !idsOfReceivedServings.Contains(ss.Id));

            _context.ServingSizes.RemoveRange(servingSizesToDelete);
            

            await _context.SaveChangesAsync();
            
            

            return NoContent();
        }
        
        [HttpPost]
        public async Task<ActionResult<FoodItem>> PostFoodItem(FoodItem foodItem)
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (tokenUserId == null)
            {
                return Unauthorized();
            }
            
            long userId = Convert.ToInt64(tokenUserId);
            
            
            // might be redundant
            var existingUser = await _context.Users
                .Where(user => user.Id == userId)
                .FirstOrDefaultAsync();

            if (existingUser == null)
            {
                return NotFound();
            }

            foodItem.UserId = userId;
            
            _context.FoodItems.Add(foodItem);
            await _context.SaveChangesAsync();
            
            var foodItemDto = new FoodItemDto
            {
                Id = foodItem.Id,
                Name = foodItem.Name,
                Energy = foodItem.Energy,
                Carbs = foodItem.Carbs,
                Fat = foodItem.Fat,
                Protein = foodItem.Protein
            };

            return CreatedAtAction(nameof(GetFoodItem), new { id = foodItem.Id }, foodItemDto);
        }

        // DELETE: api/FoodItem/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFoodItem(long id)
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (tokenUserId == null)
            {
                return Unauthorized();
            }
            
            long userId = Convert.ToInt64(tokenUserId);
            
            var foodItem = await _context.FoodItems
                .Where(fi => fi.UserId == userId && fi.Id == id).FirstOrDefaultAsync();
            
            
            // var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null)
            {
                return NotFound();
            }

            _context.FoodItems.Remove(foodItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/servingsizes")]
        public async Task<ActionResult<IEnumerable<ServingSizeDto>>> GetServingSizes(long id)
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (tokenUserId == null)
            {
                return Unauthorized();
            }
            
            long userId = Convert.ToInt64(tokenUserId);
            
            
            

            var foodItem = await _context.FoodItems
                .Where(fi => fi.UserId == userId && fi.Id == id)
                .FirstOrDefaultAsync();

            if (foodItem == null)
            {
                return NotFound();
            }

            var servingSizes = await _context.ServingSizes
                .Where(ss => ss.FoodItemId == foodItem.Id)
                .Select(ss => new ServingSizeDto
                {
                    Id = ss.Id,
                    Name = ss.Name,
                    Amount = ss.Amount
                })
                .ToListAsync();
            
            return servingSizes;
        }

        [HttpPost("{id}/servingsizes")]
        public async Task<ActionResult<ServingSizeDto>> PostServingSize(long id, ServingSize servingSize)
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (tokenUserId == null)
            {
                return Unauthorized();
            }
            
            long userId = Convert.ToInt64(tokenUserId);

            var foodItem = await _context.FoodItems
                .Where(fi => fi.UserId == userId && fi.Id == id)
                .FirstOrDefaultAsync();

            if (foodItem == null)
            {
                return NotFound();
            }

            servingSize.FoodItemId = id;
            
            _context.ServingSizes.Add(servingSize);
            await _context.SaveChangesAsync();

            var servingSizeDto = new ServingSizeDto
            {
                Id = servingSize.Id,
                Amount = servingSize.Amount,
                Name = servingSize.Name
            };
            
            return CreatedAtAction(nameof(GetServingSizes), new { id = foodItem.Id }, servingSizeDto);
        }
    }
}