namespace CalorieTrackerAPI.Models;

public class FoodIntakeDto
{
    public long Id { get; set; }
    public String? Name { get; set; }
    public long FoodItemId { get; set; }
    public double Energy { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public double Protein { get; set; }
    public long? Quantity { get; set; }
    

    public int ServingSizeIndex { get; set; }
    
    public ICollection<ServingSizeDto> AvailableServingSizes { get; set; }
}