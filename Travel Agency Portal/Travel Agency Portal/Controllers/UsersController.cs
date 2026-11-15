using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Agency_Portal.Models;
using Travel_Agency_Portal.Services;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class UsersController(UsersService service) : ApiControllerBase
{

    [Authorize(Roles = "admin,auditor")]
    [HttpGet]
    public IActionResult GetUsers()
        => ToActionResult(service.GetUsers());

    [Authorize]
    [HttpGet("{id:int}")]
    public IActionResult GetUser(int id)
        => ToActionResult(service.GetUser(id));

    [Authorize]
    [HttpPut("{id:int}")]
    public IActionResult UpdateUser(int id, [FromBody] UpdateUserProfileRequest user)
        => ToActionResult(service.UpdateUser(id, user));

    [Authorize]
    [HttpPut("{id:int}/password")]
    public IActionResult ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        => ToActionResult(service.ChangePassword(id, request));

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}")]
    public IActionResult DeleteUser(int id)
        => ToActionResult(service.DeleteUser(id));

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}/role")]
    public IActionResult UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request)
        => ToActionResult(service.UpdateUserRole(id, request));

}
