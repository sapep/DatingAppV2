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
        ))
      .ForMember(
        destinationMember => destinationMember.PhotoUrl,
        option => option.MapFrom(
          source => source.Photos.FirstOrDefault(photo => photo.IsMain)!.Url
        ))
      .ForMember(
        destinationMember => destinationMember.Photos,
        options => options.MapFrom(
          source => source.Photos.Where(photo => photo.IsApproved)
        ));

    CreateMap<Photo, PhotoDto>();

    CreateMap<MemberUpdateDto, AppUser>();

    CreateMap<RegisterDto, AppUser>();

    CreateMap<string, DateOnly>().ConvertUsing(source => DateOnly.Parse(source));

    CreateMap<Message, MessageDto>()
      .ForMember(
        destination => destination.SenderPhotoUrl,
        option => option.MapFrom(
          source => source.Sender.Photos.FirstOrDefault(photo => photo.IsMain)!.Url
        ))
      .ForMember(
        destination => destination.RecipientPhotoUrl,
        option => option.MapFrom(
          source => source.Recipient.Photos.FirstOrDefault(photo => photo.IsMain)!.Url
        ));

    CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));

    CreateMap<DateTime?, DateTime?>()
      .ConvertUsing(d => d.HasValue ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null);
  }
}
