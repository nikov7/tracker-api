namespace CalorieTrackerAPI.Models;

public class ServingSize
{
    public long Id { get; set; }
    public long FoodItemId { get; set; }
    public FoodItem? FoodItem { get; set; }
    
    public String? Name { get; set; }
    public int Amount { get; set; }
}