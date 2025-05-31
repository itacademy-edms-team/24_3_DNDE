using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App.Sheet1
{
    public class HitDiceDto
    {
        [Required]
        public int Total { get; set; }

        [Required]
        public int Current { get; set; }

        [Required]
        public string DiceType { get; set; } // "D4" | "D6" | "D8" | "D10" | "D12";
    }
}
