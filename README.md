# Guide for the use of Hv.Sos100.SingleSignOn

## Installation

In the top menu of Visual Studio select **Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution**.

Search for **Hv.Sos100.SingleSignOn** and install the latest version of the NuGet package.

## Using Single Sign On (SSO)

The SSO system is used to manage account sessions and cookies. Used correctly the system will keep the account signed in even if they navigate between different domains and websites within the systems target. The system targets `https` or `http` `localhost` and all domains ending in `.ei.hv.se`.

> [!IMPORTANT]
> All environments the SSO targets are kept seperate, so the account will not stay signed in if they nagivate from an `.ei.hv.se` site to a `localhost` site.

### Prerequisites

1. Add the using statement wherever you use code from the SSO
 
```csharp
using Hv.Sos100.SingleSignOn;
```

2. Add this code in `Program.cs`

```csharp
// Before var app = builder.Build();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddSession();
// After var app = builder.Build();
 app.UseSession();
```

## Sessions & Cookies

The code is used in three main circumstances: 
- When the account wants to start a new session
- When the account wants to resume an existing session
- When the account wants to end a session

If the account has never authenticated themselves (signed in) on their browser before or they signed out earlier, they will need to start a new session.

```csharp
// Example of case where the CreateSession method is used. The account attempts to sign in and the html form directs here
// The account need to supply their email and password in the form
public async Task<IActionResult> Login(string email, string password)
{
    // Create an instance of the authenticationservice, be careful not to mix it up with the native .NET AuthenticationService
    var authenticationService = new AuthenticationService();

    // Use the authenticationService object to call the CreateSession method and optionally store the result in a variable
    // Make sure to supply the CreateSession method with the email, password and controllerBase:this, HttpContext
    var authenticatedSession = await authenticationService.CreateSession(email, password, controllerBase:this, HttpContext);

    // Depending on if the authentication was successfull the account can be directed to different pages
    return authenticatedSession ? RedirectToAction("Index", "Home") : RedirectToAction("Privacy", "Home");
}
```

If the account has alredy authenticated themselves (signed in) on their browser, they can resume a previous session.

```csharp
public async Task<IActionResult> Index()
{
    // Create an instance of the authenticationservice, be careful not to mix it up with the native .NET AuthenticationService
    var authenticationService = new AuthenticationService();

    // Use the authenticationService object to call the ResumeSession method and optionally store the result in a variable
    // The ResumeSession method does not need to be supplied with any account information as it checks the browser cookies for a valid token
    var existingSession = await _authenticationService.ResumeSession(controllerBase:this, HttpContext);

    return View();
}
```

Once the account has a session whether is was created or resumed, they will have session variables assigned which can be used by the application.

> [!NOTE]
> The `ReadSessionVariables` method works by storing the session variables in the ViewData directory. Therefore it needs to be used in every controller method that is connected to a view who will use the session variables. 

```csharp
// Use the ReadSessionVariables method to get access to the account´s id as well as a IsAuthenticated string as a session variable
// You can then use these session variables in you html code to conditionally display information
// Or in your C# code to request data about the account from an api using the account´s id
authenticationService.ReadSessionVariables(controller: this, httpContext: HttpContext);
```
```html
// Example of using session variables in html code
@{
    var isAuthenticated = ViewData["IsAuthenticated"] as string;
    var accountId = ViewData["AccountId"] as string;
    var AccountType = ViewData["AccountType"] as string;
}
<html>
<body>
  @if (isAuthenticated != "true")
  {
    <h3>Please authenticate yourself to view the page</h3>
    }
  else
  {
    <h3>Welcome, @accountId!</h3>
  }
</body>
</html>
```

If the account wants to end their session and sign out all the data stored on their browser will be removed.

```csharp
public IActionResult Logout()
{
    // Create an instance of the authenticationservice, be careful not to mix it up with the native .NET AuthenticationService
    var authenticationService = new AuthenticationService();

    // Call the EndSession method to delete the cookie and end the account´s session
    // This will force the account to authenticate themselves again if they want to have access
    authenticationService.EndSession(controllerBase:this, HttpContext);
    return RedirectToAction("Index", "Home");
}
```
