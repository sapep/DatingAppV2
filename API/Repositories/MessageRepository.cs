using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
{
  public void AddMessage(Message message)
  {
    context.Messages.Add(message);
  }

  public void DeleteMessage(Message message)
  {
    context.Messages.Remove(message);
  }

  public async Task<Message?> GetMessage(int id)
  {
    return await context.Messages.FindAsync(id);
  }

  public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
  {
    var query = context.Messages
      .OrderByDescending(message => message.MessageSent)
      .AsQueryable();

    query = messageParams.Container switch
    {
      "Inbox" => query.Where(message => message.Recipient.UserName == messageParams.Username && !message.RecipientDeleted),
      "Outbox" => query.Where(message => message.Sender.UserName == messageParams.Username && !message.SenderDeleted),
      _ => query.Where(message => message.Recipient.UserName == messageParams.Username && message.DateRead == null && !message.RecipientDeleted)
    };

    var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

    return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
  }

  public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
  {
    var messages = await context.Messages
      .Include(message => message.Sender).ThenInclude(sender => sender.Photos)
      .Include(message => message.Recipient).ThenInclude(recipient => recipient.Photos)
      .Where(message =>
        (message.SenderUsername == currentUsername && message.RecipientUsername == recipientUsername && !message.SenderDeleted) ||
        (message.RecipientUsername == currentUsername && message.SenderUsername == recipientUsername && !message.RecipientDeleted))
      .OrderBy(message => message.MessageSent)
      .ToListAsync();

    var unreadMessages = messages.Where(message => message.DateRead == null && message.RecipientUsername == currentUsername).ToList();

    if (unreadMessages.Count > 0)
    {
      unreadMessages.ForEach(message => message.DateRead = DateTime.UtcNow);
      await context.SaveChangesAsync();
    }

    return mapper.Map<IEnumerable<MessageDto>>(messages);
  }

  public async Task<bool> SaveAllAsync()
  {
    return await context.SaveChangesAsync() > 0;
  }
}
