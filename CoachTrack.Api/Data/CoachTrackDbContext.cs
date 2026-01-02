using Microsoft.EntityFrameworkCore;

namespace CoachTrack.Api.Data;

public class CoachTrackDbContext : DbContext
{
    public CoachTrackDbContext(DbContextOptions<CoachTrackDbContext> options) : base(options) { }

    public DbSet<CoachTrack.Api.Models.Client> Clients => Set<CoachTrack.Api.Models.Client>();
    public DbSet<CoachTrack.Api.Models.CheckIn> CheckIns => Set<CoachTrack.Api.Models.CheckIn>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CoachTrack.Api.Models.Client>()
            .HasMany(c => c.CheckIns)
            .WithOne(ci => ci.Client!)
            .HasForeignKey(ci => ci.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CoachTrack.Api.Models.CheckIn>()
            .Property(ci => ci.WeightKg)
            .HasPrecision(5, 2);
    }
}
