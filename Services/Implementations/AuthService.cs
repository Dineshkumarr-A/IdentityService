using IdentityService.Entities;
using IdentityService.Models.Constants;
using IdentityService.Models.DTO;
using IdentityService.Models.Requests;
using IdentityService.Models.Responses;
using IdentityService.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace IdentityService.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        public AuthService(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            var user = new AppUser
            {
                UserName = request.Username,
                Email = request.Email,
            };

            var result = await _userManager.CreateAsync(user, request.Password!);

            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            return GenerateTokens(user.UserName!);
        }

        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username!);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password!))
                return new AuthResult { Success = false, Message = "Invalid username or password" };

            return GenerateTokens(user.UserName!);
        }

        // ✅ External Login (e.g., Google)
        public async Task<AuthResult> ExternalLoginAsync(ExternalLoginRequest request)
        {
            // 1. Validate token with external provider
            var payload = await ValidateExternalTokenAsync(request.Provider!, request.IdToken!);
            if (payload == null)
                return new AuthResult { Success = false, Message = "Invalid external token" };

            // 2. Find or create user
            var email = payload.Email;
            var user = await _userManager.FindByEmailAsync(email!);
            if (user == null)
            {
                user = new AppUser { UserName = email, Email = email };
                await _userManager.CreateAsync(user);
            }

            // 3. Issue your own tokens
            return GenerateTokens(user.UserName!);
        }

        // ✅ Refresh token (stateless JWT validation)
        public async Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var principal = ValidateJwtToken(request.RefreshToken!, TokenType.Refresh.ToString());
            if (principal == null)
                return new AuthResult { Success = false, Message = "Invalid refresh token" };

            var username = principal.Identity?.Name;
            return GenerateTokens(username!);
        }

        private AuthResult GenerateTokens(string userName) 
        {
            var accessToken = GenerateJwtToken(userName, TokenType.Access.ToString());
            var refreshToken = GenerateJwtToken(userName, TokenType.Refresh.ToString());

            return new AuthResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:AccessExpiryTime"]))
            };
        }

        private string GenerateJwtToken(string userName, string type)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? "super_secret");
            var ExpiryTime = Convert.ToInt32(type == TokenType.Access.ToString() ? _configuration["Jwt:AccessExpiry"] : _configuration["Jwt:RefreshExpiry"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim("type", type)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(ExpiryTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private ClaimsPrincipal? ValidateJwtToken(string token, string expectedType)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "super_secret");
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true
                }, out var validatedToken);

                var jwtToken = validatedToken as JwtSecurityToken;
                if (jwtToken == null || jwtToken.Claims.First(c => c.Type == "type").Value != expectedType)
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }

        private async Task<GooglePayload?> ValidateExternalTokenAsync(string provider, string idToken)
        {
            if (provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
                return JsonSerializer.Deserialize<GooglePayload>(response);
            }

            // TODO: Add support for Microsoft, GitHub, etc.
            return null;
        }

    }
}
