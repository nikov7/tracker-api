namespace CalorieTrackerAPI.Models;

public class DiaryEntry
{
    public long Id { get; set; }
    public DateOnly Date { get; set; }
    
    public long ProfileId { get; set; }
    public Profile? Profile { get; set; }
}