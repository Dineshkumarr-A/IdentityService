namespace IdentityService.Models.Responses
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime ExpiresIn { get; set; }   // expiry for access token
    }
}
