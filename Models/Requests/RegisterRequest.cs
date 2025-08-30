using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models.Requests
{
    public class RegisterRequest
    {
        [Required]
        [MinLength(3)]
        public string? Username { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        // Optional if you add SSO later, so make it Required only for local flow
        [Required(ErrorMessage = "Password is required for local registration")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }
    }
}
