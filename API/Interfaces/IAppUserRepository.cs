using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IAppUserRepository
{
  void Update(AppUser user);
  Task<IEnumerable<AppUser>> GetAllUsersAsync();
  Task<AppUser?> GetUserByIdAsync(int id);
  Task<AppUser?> GetUserByUsernameAsync(string username);
  Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
  Task<MemberDto?> GetMemberAsync(string username, string requestingUsername);
}
