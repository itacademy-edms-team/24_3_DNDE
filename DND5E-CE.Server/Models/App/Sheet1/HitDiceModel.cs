using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DND5E_CE.Server.Models.App.Sheet1
{
    public class HitDiceModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid Sheet1Id { get; set; }

        [ForeignKey("Sheet1Id")]
        public Sheet1Model Sheet1 { get; set; }

        [Required]
        public int Total { get; set; } = 1;

        [Required]
        public int Current { get; set; } = 1;

        [Required]
        public string DiceType { get; set; } = "D4"; // "D4" | "D6" | "D8" | "D10" | "D12";

    }
}
