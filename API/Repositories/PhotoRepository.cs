using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class PhotoRepository(DataContext context, IMapper mapper) : IPhotoRepository
{
  public async Task<IEnumerable<PhotoDto>> GetAllUnapprovedPhotos()
  {
    return await context.Photos
      .Where(photo => !photo.IsApproved)
      .ProjectTo<PhotoDto>(mapper.ConfigurationProvider)
      .ToListAsync();
  }

  public async Task<Photo?> GetPhoto(int photoId)
  {
    return await context.Photos.FindAsync(photoId);
  }
}
