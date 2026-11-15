using Travel_Agency_Portal.Data;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Services;

public class BookingsService : OperationService
{
    private readonly BookingsRepository _repository;
    private readonly RequestActor _actor;
    private readonly CatalogCache _catalogCache;

    public BookingsService(BookingsRepository repository, RequestActor actor, CatalogCache catalogCache)
    {
        _repository = repository;
        _actor = actor;
        _catalogCache = catalogCache;
    }

    public OperationResult GetMyBookings()
    {
        var userId = GetCurrentUserId();
        return Ok(_repository.ListForUser(userId));
    }

    public OperationResult GetBookings()
    {
        return Ok(_repository.List());
    }

    public OperationResult GetBooking(int id)
    {
        var userId = GetCurrentUserId();
        var result = _repository.FindForUser(id, userId, CanViewAllBookings());

        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    public OperationResult CreateBooking(CreateBookingRequest request)
    {
        var error = BookingRules.ValidateRequest(request);
        if (error is not null) return BadRequest(new { message = error });
        var result = _repository.CreateBooking(request, _actor.UserId ?? 0);
        if (result.Status < 300) _catalogCache.Invalidate();
        return result;
    }

    public OperationResult UpdateBookingStatus(int id, string status)
    {
        var error = BookingRules.ValidateTransition("Pending", status, false);
        if (error is not null) return BadRequest(new { message = error });
        return ChangeStatus(id, status, null, false);
    }

    public OperationResult CancelMyBooking(int id)
        => ChangeStatus(id, "Cancelled", _actor.UserId ?? 0, true);

    private OperationResult ChangeStatus(int id, string status, int? owner, bool userCancellation)
    {
        var result = _repository.ChangeBookingStatus(id, status, owner, userCancellation);
        if (result.Status < 300) _catalogCache.Invalidate();
        return result;
    }

    private int GetCurrentUserId() => _actor.UserId ?? 0;
    private bool CanViewAllBookings() => _actor.IsInRole("admin") || _actor.IsInRole("auditor");
}
