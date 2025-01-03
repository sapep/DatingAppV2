using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface ILikesRepository
{
  Task<AppUserLike?> GetUserLike(int sourceUserId, int targetUserId);
  Task<PagedList<MemberDto>> GetUserLikes(LikesParams likesParams);
  Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId);
  void DeleteLike(AppUserLike like);
  void AddLike(AppUserLike like);
  Task<bool> SaveChanges();
}
