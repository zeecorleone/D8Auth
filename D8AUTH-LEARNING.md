# D8Auth Learning Guide 📚

A step-by-step tutorial for learning ASP.NET Core Identity authentication and authorization in .NET 8, following the controller-based architecture.

---

## Table of Contents

1. [Prerequisites & Current State](#prerequisites--current-state)
2. [Module 1: Understanding the Default Identity API](#module-1-understanding-the-default-identity-api)
3. [Module 2: Custom Registration with Extended User Properties](#module-2-custom-registration-with-extended-user-properties)
4. [Module 3: Role-Based Authorization](#module-3-role-based-authorization)
5. [Module 4: Claims-Based Authorization](#module-4-claims-based-authorization)
6. [Module 5: Policy-Based Authorization](#module-5-policy-based-authorization)
7. [Module 6: Two-Factor Authentication (2FA)](#module-6-two-factor-authentication-2fa)
8. [Module 7: Email Confirmation & Password Reset](#module-7-email-confirmation--password-reset)
9. [Module 8: Custom JWT Token Generation](#module-8-custom-jwt-token-generation)
10. [Module 9: Refresh Tokens](#module-9-refresh-tokens)
11. [Module 10: OAuth/External Providers](#module-10-oauthexternal-providers)

---

## Prerequisites & Current State

### What You Already Have ✅

- .NET 8 Web API project
- ASP.NET Core Identity configured
- Custom `ApplicationUser` with:
  - `DateOfBirth` (DateOnly)
  - `Department` (string?)
- Entity Framework Core with SQL Server
- Swagger UI configured
- Default Identity API endpoints via `MapIdentityApi<ApplicationUser>()`

### Default Endpoints Available

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/register` | Register new user |
| POST | `/login` | Login user (returns JWT token) |
| POST | `/refresh` | Refresh access token |
| GET | `/confirmEmail` | Confirm user email |
| POST | `/resendConfirmationEmail` | Resend confirmation email |
| POST | `/forgotPassword` | Request password reset |
| POST | `/resetPassword` | Reset password |
| POST | `/manage/2fa` | Manage 2FA |
| GET | `/manage/info` | Get user info |
| POST | `/manage/info` | Update user info |

### Project Structure

```
D8Auth/
├── Controllers/
│   └── WeatherForecastController.cs
├── Data/
│   ├── ApplicationUser.cs          # Custom user model
│   └── DataContext.cs               # EF Core DbContext
├── Program.cs                       # App configuration
└── appsettings.json                 # Configuration
```

---

## Module 1: Understanding the Default Identity API

**Goal:** Test and understand the built-in Identity API endpoints

**Estimated Time:** 30 minutes

### Step 1.1: Test Registration

1. Run your application (`F5` or `dotnet run`)
2. Open Swagger UI: `https://localhost:<port>/swagger`
3. Find the **POST /register** endpoint
4. Click "Try it out"
5. Use this JSON:

```json
{
  "email": "test@example.com",
  "password": "Test@1234"
}
```

**Expected Result:** User created successfully

**⚠️ Problem:** Custom properties (DateOfBirth, Department) are NOT set because the default endpoint doesn't know about them!

### Step 1.2: Test Login

1. Find **POST /login** endpoint
2. Use same credentials:

```json
{
  "email": "test@example.com",
  "password": "Test@1234"
}
```

**Expected Result:** You'll receive:
```json
{
  "tokenType": "Bearer",
  "accessToken": "eyJhbGci...",
  "expiresIn": 3600,
  "refreshToken": "..."
}
```

### Step 1.3: Inspect the JWT Token

1. Copy the `accessToken`
2. Go to [jwt.io](https://jwt.io)
3. Paste the token in the "Encoded" section
4. Look at the **payload** section

**You'll see claims like:**
- `sub` (user ID)
- `email`
- `jti` (token ID)
- `exp` (expiration)

**Notice:** No custom properties (DateOfBirth, Department)!

### Step 1.4: Test Protected Endpoint

1. Open `WeatherForecastController.cs`
2. Add `[Authorize]` attribute:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace D8Auth.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    // ...existing code...

    [HttpGet(Name = "GetWeatherForecast")]
    [Authorize]  // Add this line
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
```

3. In Swagger, click the **Authorize** button (top right)
4. Enter: `Bearer <your-access-token>`
5. Test GET /WeatherForecast

**Expected Result:** 200 OK (authorized)
**Without token:** 401 Unauthorized

### ✅ Module 1 Complete!

**What You Learned:**
- How to use default Identity API endpoints
- How JWT tokens work
- How to protect endpoints with `[Authorize]`
- The limitation: custom properties aren't included

---

## Module 2: Custom Registration with Extended User Properties

**Goal:** Create a custom registration endpoint that sets DateOfBirth and Department

**Estimated Time:** 45 minutes

### Step 2.1: Create a DTO Model

Create `D8Auth/Models/RegisterRequest.cs`:

```csharp
namespace D8Auth.Models;

public class RegisterRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? Department { get; set; }
}
```

### Step 2.2: Create Account Controller

Create `D8Auth/Controllers/AccountController.cs`:

```csharp
using D8Auth.Data;
using D8Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace D8Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Validate the model
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Create user with custom properties
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth,
            Department = request.Department
        };

        // Create user in the database
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new
        {
            message = "User registered successfully",
            userId = user.Id,
            email = user.Email
        });
    }
}
```

### Step 2.3: Test Custom Registration

1. Run the app
2. In Swagger, find **POST /api/account/register**
3. Use this JSON:

```json
{
  "email": "john@example.com",
  "password": "SecurePass@123",
  "dateOfBirth": "1990-05-15",
  "department": "Engineering"
}
```

**Expected Result:**
```json
{
  "message": "User registered successfully",
  "userId": "...",
  "email": "john@example.com"
}
```

### Step 2.4: Verify in Database

Open SQL Server Management Studio or Azure Data Studio and run:

```sql
SELECT Id, Email, DateOfBirth, Department 
FROM AspNetUsers 
WHERE Email = 'john@example.com'
```

**You should see the custom properties saved!**

### Step 2.5: Add User Info Endpoint

Add this method to `AccountController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;

// Add above the class
[Authorize]
[HttpGet("me")]
public async Task<IActionResult> GetCurrentUser()
{
    var user = await _userManager.GetUserAsync(User);
    
    if (user == null)
        return NotFound();

    return Ok(new
    {
        email = user.Email,
        dateOfBirth = user.DateOfBirth,
        department = user.Department,
        age = CalculateAge(user.DateOfBirth)
    });
}

private static int CalculateAge(DateOnly dateOfBirth)
{
    var today = DateTime.Today;
    var age = today.Year - dateOfBirth.Year;
    if (today.DayOfYear < dateOfBirth.DayOfYear)
        age--;
    return age;
}
```

### Step 2.6: Test User Info

1. Login with your new user (POST /login)
2. Copy the `accessToken`
3. Authorize in Swagger (`Bearer <token>`)
4. Call GET /api/account/me

**Expected Result:**
```json
{
  "email": "john@example.com",
  "dateOfBirth": "1990-05-15",
  "department": "Engineering",
  "age": 34
}
```

### ✅ Module 2 Complete!

**What You Learned:**
- How to extend user registration with custom properties
- How to use `UserManager<T>` to create users
- How to retrieve the current authenticated user
- How to create DTOs for API requests

**Key Concepts:**
- `UserManager<ApplicationUser>` - manages user operations
- `User` property in controller - represents authenticated user
- `GetUserAsync(User)` - gets user from ClaimsPrincipal

---

## Module 3: Role-Based Authorization

**Goal:** Implement roles and restrict endpoints based on user roles

**Estimated Time:** 45 minutes

### Step 3.1: Enable Roles Support

Update `Program.cs`:

```csharp
builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options => {
    //options.SignIn.RequireConfirmedAccount = false;
    //options.Password.RequireDigit = false;
})
.AddRoles<Microsoft.AspNetCore.Identity.IdentityRole>()  // Add this line
.AddEntityFrameworkStores<DataContext>();
```

### Step 3.2: Create Migration for Roles

Run in terminal:

```bash
dotnet ef migrations add AddRolesSupport
dotnet ef database update
```

**What this does:** Creates role tables in your database (AspNetRoles, AspNetUserRoles, etc.)

### Step 3.3: Create Role Seeder Endpoint

Add to `AccountController.cs`:

```csharp
private readonly RoleManager<IdentityRole> _roleManager;

// Update constructor
public AccountController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager)
{
    _userManager = userManager;
    _roleManager = roleManager;
}

[HttpPost("seed-roles")]
public async Task<IActionResult> SeedRoles()
{
    string[] roleNames = { "Admin", "Manager", "User" };
    
    foreach (var roleName in roleNames)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    return Ok(new { message = "Roles created successfully", roles = roleNames });
}
```

### Step 3.4: Assign Roles During Registration

Update the `Register` method in `AccountController.cs`:

```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var user = new ApplicationUser
    {
        UserName = request.Email,
        Email = request.Email,
        DateOfBirth = request.DateOfBirth,
        Department = request.Department
    };

    var result = await _userManager.CreateAsync(user, request.Password);

    if (!result.Succeeded)
    {
        return BadRequest(result.Errors);
    }

    // Assign default "User" role
    await _userManager.AddToRoleAsync(user, "User");

    return Ok(new
    {
        message = "User registered successfully",
        userId = user.Id,
        email = user.Email,
        role = "User"
    });
}
```

### Step 3.5: Create Admin Assignment Endpoint

Add to `AccountController.cs`:

```csharp
[HttpPost("assign-role")]
[Authorize(Roles = "Admin")]  // Only admins can assign roles
public async Task<IActionResult> AssignRole([FromQuery] string email, [FromQuery] string role)
{
    var user = await _userManager.FindByEmailAsync(email);
    if (user == null)
        return NotFound($"User {email} not found");

    if (!await _roleManager.RoleExistsAsync(role))
        return BadRequest($"Role {role} does not exist");

    if (await _userManager.IsInRoleAsync(user, role))
        return BadRequest($"User already has {role} role");

    var result = await _userManager.AddToRoleAsync(user, role);
    
    if (!result.Succeeded)
        return BadRequest(result.Errors);

    return Ok(new { message = $"Role {role} assigned to {email}" });
}
```

### Step 3.6: Create Role-Protected Endpoints

Add these methods to `AccountController.cs`:

```csharp
[HttpGet("admin-only")]
[Authorize(Roles = "Admin")]
public IActionResult AdminOnly()
{
    return Ok(new { message = "Welcome, Admin!" });
}

[HttpGet("manager-or-admin")]
[Authorize(Roles = "Admin,Manager")]  // Multiple roles (OR logic)
public IActionResult ManagerOrAdmin()
{
    var roles = User.Claims
        .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
        .Select(c => c.Value);
    
    return Ok(new 
    { 
        message = "Welcome, Manager or Admin!",
        yourRoles = roles
    });
}
```

### Step 3.7: Testing Role-Based Authorization

**Test Sequence:**

1. **Seed Roles:**
   ```
   POST /api/account/seed-roles
   ```

2. **Register a new user:**
   ```json
   POST /api/account/register
   {
     "email": "admin@example.com",
     "password": "Admin@1234",
     "dateOfBirth": "1985-03-20",
     "department": "IT"
   }
   ```

3. **Manually make user an Admin** (in database):
   ```sql
   DECLARE @UserId NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = 'admin@example.com')
   DECLARE @RoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Admin')
   
   INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)
   ```

4. **Login as admin:**
   ```json
   POST /login
   {
     "email": "admin@example.com",
     "password": "Admin@1234"
   }
   ```

5. **Test admin endpoint:**
   ```
   GET /api/account/admin-only
   Authorization: Bearer <admin-token>
   ```
   ✅ Should return: `"Welcome, Admin!"`

6. **Test with regular user:**
   - Login as regular user
   - Try GET /api/account/admin-only
   - ❌ Should return: `403 Forbidden`

### ✅ Module 3 Complete!

**What You Learned:**
- How to enable roles in ASP.NET Core Identity
- How to create and seed roles
- How to assign roles to users
- How to protect endpoints with `[Authorize(Roles = "...")]`
- Difference between 401 Unauthorized vs 403 Forbidden

**Key Concepts:**
- `RoleManager<IdentityRole>` - manages roles
- `UserManager.AddToRoleAsync()` - assigns roles
- `[Authorize(Roles = "Admin,Manager")]` - allows multiple roles (OR)

---

## Module 4: Claims-Based Authorization

**Goal:** Use custom claims for fine-grained access control

**Estimated Time:** 60 minutes

### Step 4.1: Understanding Claims

**What are Claims?**
- Key-value pairs about a user (e.g., "Department": "Engineering")
- More flexible than roles
- Can represent permissions, attributes, or metadata

**Claims vs Roles:**
- **Roles:** User is/isn't something (Admin, Manager)
- **Claims:** User has some attribute or permission

### Step 4.2: Add Claims During Registration

Update `Register` method in `AccountController.cs`:

```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var user = new ApplicationUser
    {
        UserName = request.Email,
        Email = request.Email,
        DateOfBirth = request.DateOfBirth,
        Department = request.Department
    };

    var result = await _userManager.CreateAsync(user, request.Password);

    if (!result.Succeeded)
        return BadRequest(result.Errors);

    // Assign default role
    await _userManager.AddToRoleAsync(user, "User");

    // Add custom claims
    var claims = new List<System.Security.Claims.Claim>
    {
        new System.Security.Claims.Claim("DateOfBirth", user.DateOfBirth.ToString("yyyy-MM-dd")),
        new System.Security.Claims.Claim("Age", CalculateAge(user.DateOfBirth).ToString())
    };

    if (!string.IsNullOrEmpty(user.Department))
    {
        claims.Add(new System.Security.Claims.Claim("Department", user.Department));
    }

    await _userManager.AddClaimsAsync(user, claims);

    return Ok(new
    {
        message = "User registered successfully",
        userId = user.Id,
        email = user.Email,
        role = "User"
    });
}
```

### Step 4.3: Create Department-Based Endpoints

Add to `AccountController.cs`:

```csharp
[HttpGet("engineering-only")]
[Authorize]
public async Task<IActionResult> EngineeringOnly()
{
    var user = await _userManager.GetUserAsync(User);
    
    if (user?.Department != "Engineering")
    {
        return Forbid(); // 403 Forbidden
    }

    return Ok(new { message = "Welcome, Engineering team member!" });
}

[HttpGet("adult-only")]
[Authorize]
public IActionResult AdultOnly()
{
    var ageClaim = User.FindFirst("Age");
    
    if (ageClaim == null || !int.TryParse(ageClaim.Value, out int age) || age < 18)
    {
        return Forbid();
    }

    return Ok(new { message = "Welcome, adult user!", age });
}
```

### Step 4.4: View User Claims

Add this endpoint:

```csharp
[HttpGet("my-claims")]
[Authorize]
public IActionResult GetMyClaims()
{
    var claims = User.Claims.Select(c => new
    {
        type = c.Type,
        value = c.Value
    });

    return Ok(claims);
}
```

### Step 4.5: Testing Claims

1. **Register with Department:**
   ```json
   POST /api/account/register
   {
     "email": "engineer@example.com",
     "password": "Eng@1234",
     "dateOfBirth": "1995-07-10",
     "department": "Engineering"
   }
   ```

2. **Login and get token**

3. **View claims:**
   ```
   GET /api/account/my-claims
   ```
   
   Expected to see:
   - `DateOfBirth`: 1995-07-10
   - `Department`: Engineering
   - `Age`: 29

4. **Test department restriction:**
   ```
   GET /api/account/engineering-only
   ```
   ✅ Should work for Engineering users
   ❌ Should be 403 for other departments

### ✅ Module 4 Complete!

**What You Learned:**
- What claims are and how they differ from roles
- How to add custom claims during registration
- How to check claims in controller actions
- How to use `User.FindFirst()` and `User.Claims`

---

## Module 5: Policy-Based Authorization

**Goal:** Create reusable authorization policies

**Estimated Time:** 45 minutes

### Step 5.1: Understanding Policies

**Problem with current approach:**
```csharp
// Repeated logic in every endpoint
if (user?.Department != "Engineering") return Forbid();
```

**Solution: Authorization Policies**
- Define once, use everywhere
- Cleaner, more maintainable
- Can combine multiple requirements

### Step 5.2: Create Authorization Requirements

Create `D8Auth/Authorization/DepartmentRequirement.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;

namespace D8Auth.Authorization;

public class DepartmentRequirement : IAuthorizationRequirement
{
    public string Department { get; }

    public DepartmentRequirement(string department)
    {
        Department = department;
    }
}
```

### Step 5.3: Create Authorization Handler

Create `D8Auth/Authorization/DepartmentHandler.cs`:

```csharp
using D8Auth.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace D8Auth.Authorization;

public class DepartmentHandler : AuthorizationHandler<DepartmentRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DepartmentHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DepartmentRequirement requirement)
    {
        var user = await _userManager.GetUserAsync(context.User);

        if (user != null && user.Department == requirement.Department)
        {
            context.Succeed(requirement);
        }
    }
}
```

### Step 5.4: Create Age Requirement

Create `D8Auth/Authorization/MinimumAgeRequirement.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;

namespace D8Auth.Authorization;

public class MinimumAgeRequirement : IAuthorizationRequirement
{
    public int MinimumAge { get; }

    public MinimumAgeRequirement(int minimumAge)
    {
        MinimumAge = minimumAge;
    }
}
```

Create `D8Auth/Authorization/MinimumAgeHandler.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;

namespace D8Auth.Authorization;

public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumAgeRequirement requirement)
    {
        var ageClaim = context.User.FindFirst("Age");

        if (ageClaim != null && 
            int.TryParse(ageClaim.Value, out int age) && 
            age >= requirement.MinimumAge)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

### Step 5.5: Register Policies in Program.cs

Update `Program.cs`:

```csharp
using D8Auth.Authorization;

// After AddAuthorization(), before .AddIdentityApiEndpoints
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("EngineeringOnly", policy =>
        policy.Requirements.Add(new DepartmentRequirement("Engineering")))
    .AddPolicy("ITOnly", policy =>
        policy.Requirements.Add(new DepartmentRequirement("IT")))
    .AddPolicy("AdultOnly", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(18)))
    .AddPolicy("SeniorOnly", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(60)));

// Register handlers
builder.Services.AddScoped<IAuthorizationHandler, DepartmentHandler>();
builder.Services.AddScoped<IAuthorizationHandler, MinimumAgeHandler>();
```

### Step 5.6: Use Policies in Controllers

Update `AccountController.cs`:

```csharp
// Replace the manual check version with policy
[HttpGet("engineering-only-v2")]
[Authorize(Policy = "EngineeringOnly")]
public IActionResult EngineeringOnlyV2()
{
    return Ok(new { message = "Welcome, Engineering team! (Policy-based)" });
}

[HttpGet("adult-only-v2")]
[Authorize(Policy = "AdultOnly")]
public IActionResult AdultOnlyV2()
{
    return Ok(new { message = "Welcome, adult user! (Policy-based)" });
}

[HttpGet("it-seniors")]
[Authorize(Policy = "ITOnly")]
[Authorize(Policy = "SeniorOnly")]  // Multiple policies (AND logic)
public IActionResult ITSeniors()
{
    return Ok(new { message = "Welcome, Senior IT professional!" });
}
```

### Step 5.7: Test Policies

1. **Register Engineering user:**
   ```json
   {
     "email": "eng@example.com",
     "password": "Test@123",
     "dateOfBirth": "1995-01-01",
     "department": "Engineering"
   }
   ```

2. **Test policy endpoint:**
   ```
   GET /api/account/engineering-only-v2
   ```
   ✅ Works for Engineering department

3. **Register IT Senior:**
   ```json
   {
     "email": "senior@example.com",
     "password": "Test@123",
     "dateOfBirth": "1960-01-01",
     "department": "IT"
   }
   ```

4. **Test combined policies:**
   ```
   GET /api/account/it-seniors
   ```
   ✅ Only works if BOTH IT department AND 60+ years old

### ✅ Module 5 Complete!

**What You Learned:**
- How to create custom authorization requirements
- How to implement authorization handlers
- How to register and use policies
- How to combine multiple policies (AND logic)

**Benefits of Policies:**
- ✅ Reusable across application
- ✅ Testable in isolation
- ✅ Cleaner controller code
- ✅ Single Responsibility Principle

---

## Module 6: Two-Factor Authentication (2FA)

**Coming in next iteration** - This guide is already quite comprehensive. Would you like me to continue with Module 6, or would you prefer to work through Modules 1-5 first?

---

## Summary So Far

### Learning Path Progress

- ✅ Module 1: Default Identity API
- ✅ Module 2: Custom Registration
- ✅ Module 3: Role-Based Authorization
- ✅ Module 4: Claims-Based Authorization
- ✅ Module 5: Policy-Based Authorization
- ⏳ Module 6: Two-Factor Authentication (2FA)
- ⏳ Module 7: Email Confirmation
- ⏳ Module 8: Custom JWT Tokens
- ⏳ Module 9: Refresh Tokens
- ⏳ Module 10: OAuth/External Providers

### Key Takeaways

1. **Identity API Endpoints** - Minimal code for authentication
2. **Custom Properties** - Extend `IdentityUser` for your needs
3. **Roles** - Group-based access control
4. **Claims** - Attribute-based access control
5. **Policies** - Reusable, testable authorization logic

### Recommended Order

1. Work through Modules 1-5 completely
2. Test each module thoroughly
3. Understand the "why" before moving to "how"
4. Experiment with variations
5. Then move to advanced topics (Modules 6-10)

---

**Happy Learning! 🎉**
