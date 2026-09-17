namespace RFIDP2P3_API.Models.Request;

public class TokenApiRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}