using EcoWarriorMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoWarriorMvc.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Challenge> Challenges => Set<Challenge>();
    public DbSet<UserChallenge> UserChallenges => Set<UserChallenge>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<Reward> Rewards => Set<Reward>();
    public DbSet<AiLog> AiLogs => Set<AiLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<UserChallenge>()
            .HasOne(x => x.AppUser).WithMany(u => u.UserChallenges).HasForeignKey(x => x.AppUserId);
        modelBuilder.Entity<UserChallenge>()
            .HasOne(x => x.Challenge).WithMany(c => c.UserChallenges).HasForeignKey(x => x.ChallengeId);
    }
}
