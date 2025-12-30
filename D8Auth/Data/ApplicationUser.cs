using Microsoft.AspNetCore.Identity;

namespace D8Auth.Data;

public class ApplicationUser : IdentityUser
{
    public DateOnly DateOfBirth { get; set; }
    
    public string? Department { get; set; }
}
