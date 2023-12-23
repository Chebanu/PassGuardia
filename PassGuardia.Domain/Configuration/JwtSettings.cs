namespace PassGuardia.Domain.Configuration;

internal class JwtSettings
{
    public TimeSpan ExpiresIn { get; set; } = TimeSpan.FromMinutes(5);
    public string Secret {  get; set; }
    public string Issuer {  get; set; }
    public string Audience {  get; set; }
}
