namespace Infrastructure.Authentication;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "CourseHub";
    public string Audience { get; set; } = "CourseHubUsers";
    public int ExpiryMinutes { get; set; } = 1440;
}
