namespace CalorieTrackerAPI.Models;

public class FoodItemDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public double Energy { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public double Protein { get; set; }
    
    public virtual ICollection<ServingSizeDto>? ServingSizes { get; set; } 
}