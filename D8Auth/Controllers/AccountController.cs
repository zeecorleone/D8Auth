using D8Auth.Data;
using D8Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace D8Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth,
            Department = request.Department
        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if(!result.Succeeded)
            return BadRequest(result.Errors);

        // Assign roles if provided
        if(request.Roles is not null && request.Roles.Any())
        {
            foreach(var role in request.Roles)
            {
                if(await _roleManager.RoleExistsAsync(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
        }

        return Ok(new { Message = "User registered successfully.", UserId = user.Id, Email = user.Email });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return NotFound(new { Message = "User not found." });
        return Ok(new
        {
            UserId = user.Id,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            Department = user.Department

        });
    }

    [HttpPost("assign-role")]
    [Authorize(Roles = Constants.Roles.Admin)]
    public async Task<IActionResult> AssignRole([FromQuery] string email, [FromQuery] string role)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return NotFound($"User {email} not found");

        if (!await _roleManager.RoleExistsAsync(role))
            return BadRequest($"Role {role} doesn't exist");

        if (await _userManager.IsInRoleAsync(user, role))
            return BadRequest($"User already has role {role}");

        var result = await _userManager.AddToRoleAsync(user, role);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { Message = $"Role {role} assigned to {email}" });
    }



    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { Message = "User logged out successfully." });
    }

    [HttpPost("seed-roles")]
    public async Task<IActionResult> SeedRoles()
    {
        var roles = new[] { Constants.Roles.Admin, Constants.Roles.User, Constants.Roles.Manager };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
        return Ok(new { Message = "Roles seeded successfully." });
    }
}
