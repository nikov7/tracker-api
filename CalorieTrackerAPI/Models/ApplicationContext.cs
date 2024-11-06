using Microsoft.EntityFrameworkCore;

namespace CalorieTrackerAPI.Models;

public class ApplicationContext: DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure one-to-many relationship between User and Profile
        modelBuilder.Entity<User>()
            .HasMany(u => u.Profiles)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId);

        modelBuilder.Entity<FoodItem>()
            .HasMany(f => f.ServingSizes)
            .WithOne(s => s.FoodItem)
            .HasForeignKey(s => s.FoodItemId);
        
    }


    public DbSet<User> Users { get; set; } = null!;
    public DbSet<FoodItem> FoodItems { get; set; } = null!;

    public DbSet<DiaryEntry> DiaryEntries { get; set; } = null!;

    public DbSet<FoodIntake> FoodIntakes { get; set; } = null!;

    public DbSet<Profile> Profiles { get; set; } = null!;

    public DbSet<Preference> Preferences { get; set; } = null!;

    public DbSet<ServingSize> ServingSizes { get; set; } = null!;
    
}