namespace API.Interfaces;

public interface IUnitOfWork
{
  IAppUserRepository UserRepository { get; }
  IMessageRepository MessageRepository { get; }
  ILikesRepository LikesRepository { get; }
  IPhotoRepository PhotoRepository { get; }
  Task<bool> CompleteTransaction();
  bool HasChanges();
}
