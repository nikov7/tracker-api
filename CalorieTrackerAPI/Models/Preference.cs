namespace CalorieTrackerAPI.Models;

public class Preference
{
    public long Id { get; set; }
    public long ProfileId { get; set; }
    public Profile? Profile { get; set; }
    
    public float Energy { get; set; }
    public float Carbs { get; set; }
    public float Fat { get; set; }
    public float Protein { get; set; }
}