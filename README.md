# Guide for the use of Hv.Sos100.SingleSignOn

## Installation

In the top menu of Visual Studio select **Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution**.

Search for **Hv.Sos100.SingleSignOn** and install the latest version of the NuGet package.

## Using Single Sign On (SSO)

The SSO system is used to manage user sessions and cookies. Used correctly the system will keep the user signed in even if they navigate between different domains and websites within the systems target. The system targets `https` or `http` `localhost` and all domains ending in `.ei.hv.se`.

> [!IMPORTANT]
> All environments the SSO targets are kept seperate, so the user will not stay signed in if they nagivate from an `.ei.hv.se` site to a `localhost` site.

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
- When the user wants to start a new session
- When the user wants to resume an existing session
- When the user wants to end a session

If the user has never authenticated themselves (signed in) on their browser before or they signed out earlier, they will need to start a new session.

```csharp
// Example of case where the CreateSession method is used. The user attempts to sign in and the html form directs here
// The user need to supply their email and password in the form
public async Task<IActionResult> Login(string email, string password)
{
    // Create an instance of the authenticationservice, be careful not to mix it up with the native .NET AuthenticationService
    var authenticationService = new AuthenticationService();

    // Use the authenticationService object to call the CreateSession method and optionally store the result in a variable
    // Make sure to supply the CreateSession method with the email, password and controllerBase:this, HttpContext
    var authenticatedSession = await authenticationService.CreateSession(email, password, controllerBase:this, HttpContext);

    // Depending on if the authentication was successfull the user can be directed to different pages
    return authenticatedSession ? RedirectToAction("Index", "Home") : RedirectToAction("Privacy", "Home");
}
```

If the user has alredy authenticated themselves (signed in) on their browser, they can resume a previous session.

```csharp
public async Task<IActionResult> Index()
{
    // Create an instance of the authenticationservice, be careful not to mix it up with the native .NET AuthenticationService
    var authenticationService = new AuthenticationService();

    // Use the authenticationService object to call the ResumeSession method and optionally store the result in a variable
    // The ResumeSession method does not need to be supplied with any user information as it checks the browser cookies for a valid token
    var existingSession = await authenticationService.ResumeSession(controllerBase:this, HttpContext);

    return View();
}
```

If the user wants to end their session and sign out all the data stored on their browser will be removed.

```csharp
public IActionResult Logout()
{
    // Create an instance of the authenticationservice, be careful not to mix it up with the native .NET AuthenticationService
    var authenticationService = new AuthenticationService();

    // Call the EndSession method to delete the cookie and end the user´s session
    // This will force the user to authenticate themselves again if they want to have access
    authenticationService.EndSession(controllerBase:this, HttpContext);
    return RedirectToAction("Index", "Home");
}
```

## Consuming Session Data

Once the user has a session whether is was created or resumed, they will have session variables assigned which can be used by the application. There are two main ways of using the session variables. The first is to consume it directly in the C# controller, the other is to consume it in the HTML code.

If you want to access the data in the controller you can store the session variables as local variables in your C# code.

```csharp
// The user data can be accessed directly from your C# code by reading it from the session variables, this can only be done in the controller
var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
var userId = HttpContext.Session.GetString("UserID");
var userRole = HttpContext.Session.GetString("UserRole");
```

If you want to use the session variables in your HTML code you will have to use the ViewData directory. Fortunately there is a built in method in the NuGet package to automatically load the data to the ViewData directory. Using the `ReadSessionVariables` method all session variables will be loaded into the ViewData directory.

```csharp
// The ReadSessionVariables will store the session variables in the ViewData directory, they can then be used in the HTML code
authenticationService.ReadSessionVariables(controller: this, httpContext: HttpContext);
```

> [!NOTE]
> The `ReadSessionVariables` method works by storing the session variables in the ViewData directory. Therefore it needs to be used in every controller method that is connected to a view who will use the session variables. 

```html
// Example of using session variables in html code
@{
    var isAuthenticated = ViewData["IsAuthenticated"] as string;
    var userId = ViewData["UserID"] as string;
    var userRole = ViewData["UserRole"] as string;
}
<html>
<body>
  @if (isAuthenticated != "true")
  {
    <h3>Please authenticate yourself to view the page</h3>
    }
  else
  {
    <h3>Welcome, @userId!</h3>
  }
</body>
</html>
```
