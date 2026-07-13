namespace AselDevBlazorArchitecture.Application.Features.Users;

public sealed class CreateUserRequest
{
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string Password { get; set; } = string.Empty;
}
