using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext(DbContextOptions options) : DbContext(options)
{
  public DbSet<AppUser> AppUsers { get; set; }
  public DbSet<AppUserLike> Likes { get; set; }
  public DbSet<Message> Messages { get; set; }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);

    builder.Entity<AppUserLike>()
      .HasKey(key => new { key.SourceUserId, key.TargetUserId });

    builder.Entity<AppUserLike>()
      .HasOne(x => x.SourceUser)
      .WithMany(x => x.LikedUsers)
      .HasForeignKey(x => x.SourceUserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Entity<AppUserLike>()
      .HasOne(x => x.TargetUser)
      .WithMany(x => x.LikedByUsers)
      .HasForeignKey(x => x.TargetUserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Entity<Message>()
      .HasOne(x => x.Recipient)
      .WithMany(x => x.MessagesReceived)
      .OnDelete(DeleteBehavior.Restrict);

    builder.Entity<Message>()
      .HasOne(x => x.Sender)
      .WithMany(x => x.MessagesSent)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
