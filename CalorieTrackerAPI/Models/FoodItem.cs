namespace CalorieTrackerAPI.Models;

public class FoodItem
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User? User { get; set; }
    public string? Name { get; set; }
    public double Energy { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public double Protein { get; set; }
    
    public virtual ICollection<ServingSize>? ServingSizes { get; set; } 
}