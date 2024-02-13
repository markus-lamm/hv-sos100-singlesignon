using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hv.Sos100.SingleSignOn;

public class AuthenticationService
{
    private readonly ApiService _apiService = new();

    /// <summary>
    /// Attempt to create a new session and cookie for the user, return true if successful, false if not
    /// </summary>
    public async Task<bool> CreateSession(string username, string password, ControllerBase controllerBase, HttpContext httpContext)
    {
        var inputUser = new User { Username = username, Password = password };
        var validatedUser = await _apiService.ValidateNewAuthentication(inputUser);
        if (validatedUser == null) { return false; }

        httpContext.Session.SetString("IsAuthenticated", "true");
        httpContext.Session.SetString("UserId", validatedUser.Id.ToString());
        CreateCookie(validatedUser, controllerBase);
        return true;
    }

    /// <summary>
    /// Attempt to resume an existing signed in session, return true if successful, false if not
    /// </summary>
    public async Task<bool> ResumeSession(ControllerBase controllerBase, HttpContext httpContext)
    {
        var token = controllerBase.Request.Cookies["authenticationToken"];
        if (token == null) { return false; }

        var user = await _apiService.ValidateExistingAuthentication(token);
        if (user == null) { return false; }

        httpContext.Session.SetString("IsAuthenticated", "true");
        httpContext.Session.SetString("UserId", user.Id.ToString());
        return true;
    }

    /// <summary>
    /// End the current session and delete the cookie which holds the authentication token,
    /// signing the user out in the browser 
    /// </summary>
    public void EndSession(ControllerBase controllerBase, HttpContext httpContext)
    {
        controllerBase.Response.Cookies.Delete("authenticationToken");
        httpContext.Session.Clear();
    }

    /// <summary>
    /// Read the session variables and set them in the ViewData dictionary,
    /// sets IsAuthenticated as true and UserId as the user's id if the user is authenticated
    /// </summary>
    public void ReadSessionVariables(Controller controller, HttpContext httpContext)
    {
        var isAuthenticated = httpContext.Session.GetString("IsAuthenticated");
        controller.ViewData["IsAuthenticated"] = isAuthenticated;
        var userId = httpContext.Session.GetString("UserId");
        controller.ViewData["UserId"] = userId;
    }

    private static void CreateCookie(User user, ControllerBase controllerBase)
    {
        var isHttps = controllerBase.Request.IsHttps;
        string domain;

        if (controllerBase.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            domain = "";
            isHttps = false;
        }
        else
        {
            domain = ".ei.hv.se";
        }

        controllerBase.Response.Cookies.Append("authenticationToken", user.Token!, new CookieOptions
        {
            Secure = isHttps,
            HttpOnly = true,
            Domain = domain
        });
    }
}
