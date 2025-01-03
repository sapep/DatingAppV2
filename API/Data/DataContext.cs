using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext(DbContextOptions options) : DbContext(options)
{
  public DbSet<AppUser> AppUsers { get; set; }
  public DbSet<AppUserLike> Likes { get; set; }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);

    builder.Entity<AppUserLike>()
      .HasKey(key => new { key.SourceUserId, key.TargetUserId });

    builder.Entity<AppUserLike>()
      .HasOne(s => s.SourceUser)
      .WithMany(l => l.LikedUsers)
      .HasForeignKey(s => s.SourceUserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Entity<AppUserLike>()
      .HasOne(s => s.TargetUser)
      .WithMany(l => l.LikedByUsers)
      .HasForeignKey(s => s.TargetUserId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
