using IdentityService.Models.Requests;
using IdentityService.Models.Responses;

namespace IdentityService.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request);
        Task<AuthResult> LoginAsync(LoginRequest request);
        Task<AuthResult> ExternalLoginAsync(ExternalLoginRequest request);
        Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest request);
    }
}
