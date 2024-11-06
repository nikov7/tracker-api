namespace CalorieTrackerAPI.Models;

public class User
{
    public long Id { get; set; }
    public String? Name { get; set; }
    
    public virtual ICollection<Profile>? Profiles { get; set; }
}