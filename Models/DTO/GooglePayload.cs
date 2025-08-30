using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models.DTO
{
    public class GooglePayload
    {
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Sub { get; set; } // Google User ID
    }
}
