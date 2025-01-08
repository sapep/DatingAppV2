using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext(DbContextOptions options) :
IdentityDbContext<
  AppUser,
  AppRole,
  int,
  IdentityUserClaim<int>,
  AppUserRole,
  IdentityUserLogin<int>,
  IdentityRoleClaim<int>,
  IdentityUserToken<int>
>(options)
{
  public DbSet<AppUserLike> Likes { get; set; }
  public DbSet<Message> Messages { get; set; }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);

    builder.Entity<AppUser>()
      .HasMany(ur => ur.UserRoles)
      .WithOne(u => u.User)
      .HasForeignKey(ur => ur.UserId)
      .IsRequired();

    builder.Entity<AppRole>()
      .HasMany(ur => ur.UserRoles)
      .WithOne(u => u.Role)
      .HasForeignKey(ur => ur.RoleId)
      .IsRequired();

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
