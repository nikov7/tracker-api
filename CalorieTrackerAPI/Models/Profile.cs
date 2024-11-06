namespace CalorieTrackerAPI.Models;

public class Profile
{
    public long Id { get; set; }
    public String? Name { get; set; }
    
    public long UserId { get; set; }
    public User? User { get; set; }
}