using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.Models.App.Sheet1
{
    public class GlobalDamageModifierModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid Sheet1Id { get; set; }

        [Required]
        public Sheet1Model Sheet1 { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        // Examples: "1d6", "1d4", "2d8", ...
        public string DamageDice { get; set; } = string.Empty;

        [Required]
        // Examples: "1d6", "1d4", "2d8", ...
        public string CriticalDamageDice { get; set; } = string.Empty;

        [Required]
        // Examples: "fire", "slashing", "piercing", ...
        public string Type { get; set; } = string.Empty;

        [Required]
        public bool IsIncluded { get; set; } = false;
    }
}
