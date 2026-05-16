using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookQuotes.Api.Controllers;

public abstract class BaseController : ControllerBase
{
    // Safely extract user ID from JWT
    protected int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null)
            throw new UnauthorizedAccessException("Invalid or missing token.");

        return int.Parse(claim.Value);
    }

    // extract username from JWT
    protected string GetUserName()
    {
        var claim = User.FindFirst(ClaimTypes.Name);

        if (claim == null)
            throw new UnauthorizedAccessException("Invalid or missing token.");

        return claim.Value;
    }
}
