using AselDevBlazorArchitecture.Application.Common;
using AselDevBlazorArchitecture.Application.Features.Auth;
using AselDevBlazorArchitecture.Application.Features.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AselDevBlazorArchitecture.Web.Controllers;

[ApiController]
[Route("api/browser-session")]
public sealed class BrowserSessionController : ControllerBase
{
    private readonly IBrowserSignInService _browserSignIn;

    public BrowserSessionController(IBrowserSignInService browserSignIn)
    {
        _browserSignIn = browserSignIn;
    }

    [AllowAnonymous]
    [HttpPost("sign-in")]
    public async Task<ActionResult<ServiceResponse<UserSessionDto>>> SignIn(LoginDto request)
    {
        var response = await _browserSignIn.SignInAsync(request);
        return StatusCode(response.StatusCode, response);
    }

    [AllowAnonymous]
    [HttpPost("sign-out")]
    public async Task<IActionResult> EndSession()
    {
        await _browserSignIn.SignOutAsync();
        return NoContent();
    }
}
