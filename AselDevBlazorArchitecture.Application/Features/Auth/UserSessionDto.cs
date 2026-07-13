namespace AselDevBlazorArchitecture.Application.Features.Auth;

public sealed record UserSessionDto(
    bool IsAuthenticated,
    string UserId,
    string DisplayName,
    string Email,
    IReadOnlyList<string> Roles)
{
    public static UserSessionDto Anonymous { get; } =
        new(false, string.Empty, "Guest", string.Empty, Array.Empty<string>());
}
