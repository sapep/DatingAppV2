using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class AppUserRepository(DataContext context, IMapper mapper) : IAppUserRepository
{
  public async Task<IEnumerable<AppUser>> GetAllUsersAsync()
  {
    return await context.AppUsers
      .Include(user => user.Photos)
      .ToListAsync();
  }

    public async Task<MemberDto?> GetMemberAsync(string username)
    {
      return await context.AppUsers
        .Where(user => user.UserName == username)
        .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
        .SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<MemberDto>> GetMembersAsync()
    {
      return await context.AppUsers
        .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
        .ToListAsync();
    }

    public async Task<AppUser?> GetUserByIdAsync(int id)
  {
    return await context.AppUsers.FindAsync(id);
  }

  public async Task<AppUser?> GetUserByUsernameAsync(string username)
  {
    return await context.AppUsers
      .Include(user => user.Photos)
      .SingleOrDefaultAsync(user => user.UserName == username);
  }

  public async Task<bool> SaveAllAsync()
  {
    return await context.SaveChangesAsync() > 0;
  }

  public void Update(AppUser user)
  {
    context.Entry(user).State = EntityState.Modified;
  }
}
