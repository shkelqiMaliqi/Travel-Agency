using Microsoft.AspNetCore.Mvc;
using Travel_Agency_Portal.Services;

namespace Travel_Agency_Portal.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected static IActionResult ToActionResult(OperationResult result)
        => result.Location is not null
            ? new CreatedResult(result.Location, result.Value)
            : new ObjectResult(result.Value) { StatusCode = result.Status };
}
