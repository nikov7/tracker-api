namespace CalorieTrackerAPI.Models;

public class UserDto
{
    public long Id { get; set; }
    public String? Name { get; set; }
    
    public List<ProfileDto> Profiles { get; set; }
}