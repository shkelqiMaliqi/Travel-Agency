using Travel_Agency_Portal.Data;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Services;

public class UsersService : OperationService
{
    private readonly UsersRepository _repository;
    private readonly RequestActor _actor;

    public UsersService(UsersRepository repository, RequestActor actor)
    {
        _repository = repository;
        _actor = actor;
    }

    public OperationResult GetUsers()
    {
        return Ok(_repository.List());
    }

    public OperationResult GetUser(int id)
    {
        if (!CanViewUser(id))
        {
            return Forbid();
        }

        var result = _repository.Find(id);

        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    public OperationResult UpdateUser(int id, UpdateUserProfileRequest user)
    {
        if (!CanAccessUser(id))
        {
            return Forbid();
        }

        var duplicateCount = _repository.CountConflicts(id, user);

        if (duplicateCount > 0)
        {
            return Conflict(new { message = "Another user already has this email or username." });
        }

        var rows = _repository.Update(id, user);

        return rows > 0 ? Ok(new { message = "User updated successfully." }) : NotFound();
    }

    public OperationResult ChangePassword(int id, ChangePasswordRequest request)
    {
        var currentUserId = _actor.UserId?.ToString();
        if (!string.Equals(currentUserId, id.ToString(), StringComparison.Ordinal))
        {
            return Forbid();
        }

        if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return BadRequest(new { message = "Passwords do not match." });
        }

        if (!PasswordPolicy.IsValid(request.NewPassword))
        {
            return BadRequest(new { message = PasswordPolicy.Message });
        }

        var storedHash = _repository.GetPasswordHash(id, request);
        if (string.IsNullOrWhiteSpace(storedHash))
        {
            return NotFound();
        }

        if (!PasswordHasher.Verify(request.CurrentPassword, storedHash))
        {
            return BadRequest(new { message = "Current password is incorrect." });
        }

        if (PasswordHasher.Verify(request.NewPassword, storedHash))
        {
            return BadRequest(new { message = "New password must be different from the current password." });
        }

        var nextHash = PasswordHasher.Hash(request.NewPassword);
        var rows = _repository.SetPasswordHash(id, request, nextHash);

        return rows > 0 ? Ok(new { message = "Password changed successfully." }) : NotFound();
    }

    public OperationResult DeleteUser(int id)
    {
        var currentUserId = _actor.UserId?.ToString();
        if (string.Equals(currentUserId, id.ToString(), StringComparison.Ordinal))
        {
            return BadRequest(new { message = "You cannot delete your own account." });
        }

        if (_repository.CountDependencies(id) > 0)
        {
            return Conflict(new { message = "This user has bookings or contact messages and cannot be deleted." });
        }

        var rows = _repository.Delete(id);

        return rows > 0 ? Ok(new { message = "User deleted successfully." }) : NotFound();
    }

    public OperationResult UpdateUserRole(int id, UpdateUserRoleRequest request)
    {
        var currentUserId = _actor.UserId?.ToString();
        if (string.Equals(currentUserId, id.ToString(), StringComparison.Ordinal) && request.U_Type != "admin")
        {
            return BadRequest(new { message = "You cannot remove admin access from your own account." });
        }

        var rows = _repository.UpdateRole(id, request);

        return rows > 0 ? Ok(new { message = "User role updated successfully." }) : NotFound();
    }

    private bool CanAccessUser(int id)
    {
        var currentUserId = _actor.UserId?.ToString();
        var isAdmin = _actor.IsInRole("admin");

        return isAdmin || string.Equals(currentUserId, id.ToString(), StringComparison.Ordinal);
    }

    private bool CanViewUser(int id)
    {
        var currentUserId = _actor.UserId?.ToString();
        var canViewAllUsers = _actor.IsInRole("admin") || _actor.IsInRole("auditor");

        return canViewAllUsers || string.Equals(currentUserId, id.ToString(), StringComparison.Ordinal);
    }

}
