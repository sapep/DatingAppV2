using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(
  UserManager<AppUser> userManager,
  ITokenService tokenService,
  IMapper mapper
) : BaseApiController
{
  [HttpPost("register")]
  public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
  {
    if (await UserExists(registerDto.Username)) return BadRequest("Username already taken.");

    var newAppUser = mapper.Map<AppUser>(registerDto);
    newAppUser.UserName = registerDto.Username.ToLower();

    var result = await userManager.CreateAsync(newAppUser);

    if (!result.Succeeded) return BadRequest(result.Errors);

    return Ok(
      new UserDto
      {
        Username = newAppUser.UserName,
        Token = await tokenService.CreateToken(newAppUser),
        KnownAs = newAppUser.KnownAs,
        Gender = newAppUser.Gender
      }
    );
  }

  [HttpPost("login")]
  public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
  {
    var appUser = await userManager.Users
      .Include(user => user.Photos)
      .FirstOrDefaultAsync(user => user.NormalizedUserName == loginDto.Username.ToUpper());

    if (appUser == null || appUser.UserName == null) return Unauthorized("Invalid username.");

    var result = await userManager.CheckPasswordAsync(appUser, loginDto.Password);

    if (!result) return Unauthorized();

    return Ok(
      new UserDto
      {
        Username = appUser.UserName,
        Token = await tokenService.CreateToken(appUser),
        PhotoUrl = appUser.Photos.FirstOrDefault(photo => photo.IsMain)?.Url,
        KnownAs = appUser.KnownAs,
        Gender = appUser.Gender
      }
    );
  }

  private async Task<bool> UserExists(string username)
  {
    return await userManager.Users.AnyAsync(user => user.NormalizedUserName == username.ToUpper());
  }
}
