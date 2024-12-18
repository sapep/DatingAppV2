using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(
  DataContext context,
  ITokenService tokenService,
  IMapper mapper
) : BaseApiController
{
  [HttpPost("register")]
  public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
  {
    if (await UserExists(registerDto.Username)) return BadRequest("Username already taken.");

    using var hmac = new HMACSHA512();

    var newAppUser = mapper.Map<AppUser>(registerDto);
    newAppUser.UserName = registerDto.Username.ToLower();
    newAppUser.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
    newAppUser.PasswordSalt = hmac.Key;

    context.AppUsers.Add(newAppUser);
    await context.SaveChangesAsync();

    return Ok(
      new UserDto
      {
        Username = newAppUser.UserName,
        Token = tokenService.CreateToken(newAppUser),
        KnownAs = newAppUser.KnownAs,
        Gender = newAppUser.Gender
      }
    );
  }

  [HttpPost("login")]
  public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
  {
    var appUser = await context.AppUsers
      .Include(user => user.Photos)
      .FirstOrDefaultAsync(user => user.UserName.ToLower() == loginDto.Username.ToLower());

    if (appUser == null) return Unauthorized("Invalid username.");

    using var hmac = new HMACSHA512(appUser.PasswordSalt);

    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

    for (int i = 0; i < computedHash.Length; i++)
    {
      if (computedHash[i] != appUser.PasswordHash[i]) return Unauthorized("Invalid password.");
    }

    return Ok(
      new UserDto
      {
        Username = appUser.UserName,
        Token = tokenService.CreateToken(appUser),
        PhotoUrl = appUser.Photos.FirstOrDefault(photo => photo.IsMain)?.Url,
        KnownAs = appUser.KnownAs,
        Gender = appUser.Gender
      }
    );
  }

  private async Task<bool> UserExists(string username)
  {
    return await context.AppUsers.AnyAsync(user => user.UserName.ToLower() == username.ToLower());
  }
}
