namespace D8Auth.Models;

public class UserRegistrationRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? Department { get; set; }
    public List<string> Roles { get; set; } = new();
}