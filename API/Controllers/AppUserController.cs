using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class AppUserController(IAppUserRepository userRepository) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        var appUsers = await userRepository.GetMembersAsync();
        return Ok(appUsers);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<AppUser>> GetUser(string username)
    {
        var appUser = await userRepository.GetMemberAsync(username);

        if (appUser == null) return NotFound();

        return Ok(appUser);
    }
}
