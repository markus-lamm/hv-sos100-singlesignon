using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hv.Sos100.SingleSignOn;

public class AuthenticationService
{
    private readonly ApiService _apiService = new();

    /// <summary>
    /// Attempt to create a new session and cookie for the account, return true if successful, false if not
    /// </summary>
    public async Task<bool> CreateSession(string email, string password, ControllerBase controllerBase, HttpContext httpContext)
    {
        var user = new User { Email = email, Password = password };
        var authentication = await _apiService.ValidateNewSession(user);
        if (authentication == null) { return false; }

        httpContext.Session.SetString("IsAuthenticated", "true");
        httpContext.Session.SetString("UserID", authentication.UserID!);
        httpContext.Session.SetString("UserRole", authentication.UserRole!);
        CreateCookie(authentication, controllerBase);
        return true;
    }

    /// <summary>
    /// Attempt to resume an existing signed in session, return true if successful or already authenticated, false if not
    /// </summary>
    public async Task<bool> ResumeSession(ControllerBase controllerBase, HttpContext httpContext)
    {
        var alreadyAuthenticated = httpContext.Session.GetString("IsAuthenticated");
        if (alreadyAuthenticated != null) { return true; }
        
        var token = controllerBase.Request.Cookies["hv-sos100-token"];
        if (token == null) { return false; }

        var authentication = await _apiService.ValidateExistingSession(token);
        if (authentication == null) { return false; }

        httpContext.Session.SetString("IsAuthenticated", "true");
        httpContext.Session.SetString("UserID", authentication.UserID!);
        httpContext.Session.SetString("UserRole", authentication.UserRole!);
        return true;
    }

    /// <summary>
    /// End the current session and delete the cookie which holds the authentication token,
    /// signing the user out in the browser 
    /// </summary>
    public void EndSession(ControllerBase controllerBase, HttpContext httpContext)
    {
        var isHttps = controllerBase.Request.IsHttps;

        var domain = controllerBase.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ? "" : ".ei.hv.se";

        controllerBase.Response.Cookies.Delete("hv-sos100-token", new CookieOptions
        {
            Secure = isHttps,
            HttpOnly = true,
            Domain = domain
        });
        httpContext.Session.Clear();
    }

    /// <summary>
    /// Read the session variables and set them in the ViewData dictionary,
    /// sets IsAuthenticated as true, AccountId and AccountType to the values provided by the api server
    /// </summary>
    public void ReadSessionVariables(Controller controller, HttpContext httpContext)
    {
        var isAuthenticated = httpContext.Session.GetString("IsAuthenticated");
        if (isAuthenticated != null)
        {
            controller.ViewData["IsAuthenticated"] = isAuthenticated;
        }

        var userId = httpContext.Session.GetString("UserID");
        if (userId != null)
        {
            controller.ViewData["UserID"] = userId;
        }

        var userRole = httpContext.Session.GetString("UserRole");
        if (userRole != null)
        {
            controller.ViewData["UserRole"] = userRole;
        }
    }

    private static void CreateCookie(Authentication authentication, ControllerBase controllerBase)
    {
        var isHttps = controllerBase.Request.IsHttps;

        var domain = controllerBase.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ? "" : ".ei.hv.se";

        controllerBase.Response.Cookies.Append("hv-sos100-token", authentication.Token!, new CookieOptions
        {
            Secure = isHttps,
            HttpOnly = true,
            Domain = domain
        });
    }
}
