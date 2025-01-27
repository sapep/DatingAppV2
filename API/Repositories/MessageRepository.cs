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
  public void AddGroup(Group group)
  {
    context.Groups.Add(group);
  }

  public void AddMessage(Message message)
  {
    context.Messages.Add(message);
  }

  public void DeleteMessage(Message message)
  {
    context.Messages.Remove(message);
  }

  public async Task<Connection?> GetConnection(string connectionId)
  {
    return await context.Connections.FindAsync(connectionId);
  }

    public async Task<Group?> GetGroupForConnection(string connectionId)
    {
      return await context.Groups
        .Include(x => x.Connections)
        .Where(x => x.Connections.Any(y => y.ConnectionId == connectionId))
        .FirstOrDefaultAsync();
    }

    public async Task<Message?> GetMessage(int id)
  {
    return await context.Messages.FindAsync(id);
  }

  public async Task<Group?> GetMessageGroup(string groupName)
  {
    return await context.Groups
      .Include(x => x.Connections)
      .FirstOrDefaultAsync(x => x.Name == groupName);
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
    var query = context.Messages
      .Where(message =>
        (message.SenderUsername == currentUsername && message.RecipientUsername == recipientUsername && !message.SenderDeleted) ||
        (message.RecipientUsername == currentUsername && message.SenderUsername == recipientUsername && !message.RecipientDeleted))
      .OrderBy(message => message.MessageSent)
      .AsQueryable();

    var unreadMessages = query.Where(message => message.DateRead == null && message.RecipientUsername == currentUsername).ToList();

    if (unreadMessages.Count > 0)
    {
      unreadMessages.ForEach(message => message.DateRead = DateTime.UtcNow);
    }

    return await query.ProjectTo<MessageDto>(mapper.ConfigurationProvider).ToListAsync();
  }

  public void RemoveConnection(Connection connection)
  {
    context.Connections.Remove(connection);
  }
}
