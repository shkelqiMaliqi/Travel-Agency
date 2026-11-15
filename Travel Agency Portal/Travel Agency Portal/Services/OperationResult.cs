namespace Travel_Agency_Portal.Services;

// Application results are independent of MVC. Controllers translate these into HTTP.
public sealed record OperationResult(int Status, object? Value = null, string? Location = null);

public abstract class OperationService
{
    protected static OperationResult Ok(object? value = null) => new(200, value);
    protected static OperationResult BadRequest(object? value = null) => new(400, value);
    protected static OperationResult Unauthorized(object? value = null) => new(401, value);
    protected static OperationResult Forbid() => new(403);
    protected static OperationResult NotFound(object? value = null) => new(404, value);
    protected static OperationResult Conflict(object? value = null) => new(409, value);
}

public sealed record RequestActor(int? UserId, IReadOnlySet<string> Roles)
{
    public bool IsInRole(string role) => Roles.Contains(role);
}
