using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Repositories;
using API.Services;
using API.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
  public static IServiceCollection AddApplicationServices(
    this IServiceCollection services,
    IConfiguration config
  )
  {
    services.AddControllers();
    services.AddDbContext<DataContext>(options =>
    {
      options.UseSqlite(config.GetConnectionString("DefaultConnection"));
    });
    services.AddCors();

    services.AddScoped<ITokenService, TokenService>();

    services.AddScoped<IAppUserRepository, AppUserRepository>();
    services.AddScoped<ILikesRepository, LikesRepository>();
    services.AddScoped<IMessageRepository, MessageRepository>();
    services.AddScoped<IPhotoRepository, PhotoRepository>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();

    services.AddScoped<IPhotoService, PhotoService>();
    services.AddScoped<LogUserActivity>();

    services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
    services.AddSignalR();
    services.AddSingleton<PresenceTracker>();

    return services;
  }
}
