using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
  public AutoMapperProfiles()
  {
    CreateMap<AppUser, MemberDto>()
      .ForMember(
        destination => destination.Age,
        option => option.MapFrom(
          source => source.DateOfBirth.CalculateAge()
        )
      )
      .ForMember(
        destinationMember => destinationMember.PhotoUrl,
        option => option.MapFrom(
          source => source.Photos.FirstOrDefault(photo => photo.IsMain)!.Url
        )
      );

    CreateMap<Photo, PhotoDto>();

    CreateMap<MemberUpdateDto, AppUser>();
  }
}
