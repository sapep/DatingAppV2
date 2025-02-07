using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IPhotoRepository
{
  Task<IEnumerable<PhotoDto>> GetAllUnapprovedPhotos();
  Task<Photo?> GetPhoto(int photoId);
}
