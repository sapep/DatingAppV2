using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class AppUserRepository(DataContext context, IMapper mapper) : IAppUserRepository
{
  public async Task<IEnumerable<AppUser>> GetAllUsersAsync()
  {
    return await context.Users
      .Include(user => user.Photos)
      .ToListAsync();
  }

    public async Task<MemberDto?> GetMemberAsync(string username)
    {
      return await context.Users
        .Where(user => user.UserName == username)
        .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
        .SingleOrDefaultAsync();
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
      var query = context.Users.AsQueryable();

      query = query.Where(appUser => appUser.UserName != userParams.CurrentUsername);

      if (userParams.Gender != null)
      {
        query = query.Where(appUser => appUser.Gender == userParams.Gender);
      }

      var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
      var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

      query = query.Where(appUser => appUser.DateOfBirth >= minDob && appUser.DateOfBirth <= maxDob);

      query = userParams.OrderBy switch
      {
        "created" => query.OrderByDescending(appUser => appUser.Created),
        _ => query.OrderByDescending(appUser => appUser.LastActive)
      };

      return await PagedList<MemberDto>.CreateAsync(
        query.ProjectTo<MemberDto>(mapper.ConfigurationProvider),
        userParams.PageNumber,
        userParams.PageSize
      );
    }

    public async Task<AppUser?> GetUserByIdAsync(int id)
  {
    return await context.Users.FindAsync(id);
  }

  public async Task<AppUser?> GetUserByUsernameAsync(string username)
  {
    return await context.Users
      .Include(user => user.Photos)
      .SingleOrDefaultAsync(user => user.UserName == username);
  }

  public void Update(AppUser user)
  {
    context.Entry(user).State = EntityState.Modified;
  }
}
