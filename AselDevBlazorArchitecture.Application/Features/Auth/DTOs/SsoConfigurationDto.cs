namespace AselDevBlazorArchitecture.Application.Features.Auth.DTOs
{
    public class SsoConfigurationDto
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string TokenEndpoint { get; set; } = string.Empty;
        public string UserInfoEndpoint { get; set; } = string.Empty;
        public string Scheme { get; set; } = "Bearer";
    }
}
