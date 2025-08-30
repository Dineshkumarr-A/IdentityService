using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models.Requests
{
    public class RefreshTokenRequest
    {
        [Required]
        public string? RefreshToken { get; set; }
    }
}
