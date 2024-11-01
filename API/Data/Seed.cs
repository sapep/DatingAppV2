using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
  public static async Task SeedUsers(DataContext context, IWebHostEnvironment environment)
  {
    // Seeding only allowed in development.
    if (!environment.IsDevelopment()) return;

    // Do not seed if users already exist in DB.
    if (await context.AppUsers.AnyAsync()) return;

    var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
    
    var jsonOptions = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };

    var users = JsonSerializer.Deserialize<List<AppUser>>(userData, jsonOptions);

    if (users == null) return;
    
    foreach (var user in users)
    {
      using var hmac = new HMACSHA512();

      user.UserName = user.UserName.ToLower();
      user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("password"));
      user.PasswordSalt = hmac.Key;

      context.AppUsers.Add(user);
    }

    await context.SaveChangesAsync();
  }
}
