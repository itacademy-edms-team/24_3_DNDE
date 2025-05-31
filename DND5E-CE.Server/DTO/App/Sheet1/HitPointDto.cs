using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App.Sheet1
{
    public class HitPointDto
    {
        [Required]
        public int Max { get; set; }

        [Required]
        public int Current { get; set; }

        [Required]
        public int Temp { get; set; }
    }
}
