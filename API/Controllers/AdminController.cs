using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController(
  UserManager<AppUser> userManager,
  IUnitOfWork unitOfWork,
  IMapper mapper
) : BaseApiController
{
  [Authorize(Policy = "RequireAdminRole")]
  [HttpGet("users-with-roles")]
  public async Task<ActionResult> GetUsersWithRoles()
  {
    var users = await userManager.Users
      .OrderBy(user => user.UserName)
      .Select(user => new
      {
        user.Id,
        Username = user.UserName,
        Roles = user.UserRoles.Select(userRole => userRole.Role.Name).ToList()
      }).ToListAsync();

    return Ok(users);
  }

  [Authorize(Policy = "RequireAdminRole")]
  [HttpPost("edit-roles/{username}")]
  public async Task<ActionResult> EditRoles(string username, string roles)
  {
    if (string.IsNullOrEmpty(roles)) return BadRequest("You must select atleast one role");

    var selectedRoles = roles.Split(",").ToArray();

    var user = await userManager.FindByNameAsync(username);

    if (user == null) return BadRequest("User not found");

    var userRoles = await userManager.GetRolesAsync(user);

    var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

    if (!result.Succeeded) return BadRequest("Failed to add to roles");

    result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

    if (!result.Succeeded) return BadRequest("Failed to remove from roles");

    return Ok(await userManager.GetRolesAsync(user));
  }

  [Authorize(Policy = "ModeratePhotoRole")]
  [HttpGet("photos-for-moderation")]
  public async Task<ActionResult<IEnumerable<PhotoDto>>> GetPhotosForModeration()
  {
    var photos = await unitOfWork.PhotoRepository.GetAllUnapprovedPhotos();
    return Ok(photos);
  }

  [Authorize(Policy = "ModeratePhotoRole")]
  [HttpPut("approve-photo/{photoId}")]
  public async Task<ActionResult<PhotoDto>> ApprovePhoto(int photoId)
  {
    var photo = await unitOfWork.PhotoRepository.GetPhoto(photoId);

    if (photo == null) return BadRequest("Photo does not exist");

    var user = await userManager.FindByIdAsync(photo.AppUserId.ToString());

    if (user == null) return BadRequest("User associated with the photo does not exist.");

    var photoCount = user.Photos.Count;

    if (photoCount == 1)
    {
      photo.IsMain = true;
    }

    photo.IsApproved = true;

    await unitOfWork.CompleteTransaction();

    var photoDto = mapper.Map<PhotoDto>(photo);

    return Ok(photoDto);
  }

  [Authorize(Policy = "ModeratePhotoRole")]
  [HttpPut("disapprove-photo/{photoId}")]
  public async Task<ActionResult<PhotoDto>> DisapprovePhoto(int photoId)
  {
    var photo = await unitOfWork.PhotoRepository.GetPhoto(photoId);

    if (photo == null) return BadRequest("Photo does not exist");

    photo.IsApproved = false;

    await unitOfWork.CompleteTransaction();

    var photoDto = mapper.Map<PhotoDto>(photo);
    
    return Ok(photoDto);
  }
}
