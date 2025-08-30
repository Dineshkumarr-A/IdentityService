using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models.Requests
{
    public class ExternalLoginRequest
    {
        [Required]
        public string? Provider { get; set; }   // e.g., "Google", "Microsoft", "GitHub"
        [Required]
        public string? IdToken { get; set; }    // token received from provider
    }
}
