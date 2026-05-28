using System.Security.Claims;
using DegeneraTube.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DegeneraTube.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 
    protected IActionResult FromResult<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : Error(result.Error!);
 
    protected IActionResult FromResult(Result result) =>
        result.IsSuccess ? Ok() : Error(result.Error!);
 
    private IActionResult Error(string message) =>
        BadRequest(new { error = message });
}