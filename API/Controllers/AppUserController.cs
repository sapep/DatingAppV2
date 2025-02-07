using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class AppUserController(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IPhotoService photoService
) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
    {
        userParams.CurrentUsername = User.GetUsername();
        var appUsers = await unitOfWork.UserRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(appUsers);

        return Ok(appUsers);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<AppUser>> GetUser(string username)
    {
        var requestingUsername = User.GetUsername();
        var appUser = await unitOfWork.UserRepository.GetMemberAsync(username, requestingUsername);

        if (appUser == null) return NotFound();

        return Ok(appUser);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return BadRequest("Could not find user.");

        mapper.Map(memberUpdateDto, user);
        if (await unitOfWork.CompleteTransaction()) return NoContent();

        return BadRequest("Failed to update the user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return BadRequest("Cannot update user");

        var result = await photoService.AddPhotoAsync(file);
        if (result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
            IsApproved = false
        };

        user.Photos.Add(photo);

        if (await unitOfWork.CompleteTransaction())
        {
            return CreatedAtAction(
                nameof(GetUser),
                new { username = user.UserName },
                mapper.Map<PhotoDto>(photo)
            );
        }

        return BadRequest("Problem adding photo");
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return BadRequest("Could not find user");

        var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);
        if (photo == null || photo.IsMain || !photo.IsApproved) return BadRequest("Cannot use selected photo as main photo");

        var currentMain = user.Photos.FirstOrDefault(photo => photo.IsMain);
        if (currentMain != null) currentMain.IsMain = false;

        photo.IsMain = true;

        if (await unitOfWork.CompleteTransaction()) return NoContent();

        return BadRequest("Problem setting main photo");
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return BadRequest("Could not find user");

        var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);
        if (photo == null || photo.IsMain) return BadRequest("Cannot delete non-existing or main photo");

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if (await unitOfWork.CompleteTransaction()) return Ok();

        return BadRequest("Problem deleting a photo");
    }
}
