using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App.Sheet1
{
    public class DeathSaveThrowDto
    {
        [Required]
        public int SuccessTotal { get; set; }

        [Required]
        public int FailuresTotal { get; set; }
    }
}
