using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.Auth
{
    public class PasswordChangeDto
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        public string NewPasswordConfirm { get; set; }
    }
}
