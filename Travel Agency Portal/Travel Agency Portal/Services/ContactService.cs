using Travel_Agency_Portal.Data;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Services;

public class ContactService : OperationService
{
    private readonly ContactRepository _repository;
    private readonly RequestActor _actor;

    public ContactService(ContactRepository repository, RequestActor actor)
    {
        _repository = repository;
        _actor = actor;
    }

    public OperationResult CreateMessage(ContactMessage message)
    {
        _repository.Create(message, _actor.UserId);
        return Ok(new { message = "Your message has been sent to the travel agency staff. We will respond as fast as we can." });
    }

    public OperationResult GetMessages() => Ok(_repository.GetMessages());

    public OperationResult MarkAsRead(int id)
    {
        var rows = _repository.MarkAsRead(id);
        return rows > 0 ? Ok(new { message = "Message marked as read." }) : NotFound();
    }

    public OperationResult ArchiveMessage(int id)
    {
        var rows = _repository.Archive(id);

        return rows > 0 ? Ok(new { message = "Message archived." }) : NotFound();
    }

    public OperationResult DeleteMessage(int id)
    {
        var rows = _repository.Delete(id);

        return rows > 0 ? Ok(new { message = "Message deleted." }) : NotFound();
    }

    private int? GetCurrentUserId()
    {
        var value = _actor.UserId?.ToString();
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
