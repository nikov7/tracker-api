namespace CalorieTrackerAPI.Models;

public class FoodIntake
{
    public long Id { get; set; }
    public long DiaryEntryId { get; set; }
    public DiaryEntry? DiaryEntry { get; set; }
    public long FoodItemId { get; set; }
    public FoodItem? FoodItem { get; set; }
    public long? Quantity { get; set; } // how many grams
    
    public long? ServingSizeId { get; set; }
    public ServingSize? ServingSize { get; set; }
}