using DND5E_CE.Server.Models.App.Sheet1;
using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App.Sheet1
{
    public class GlobalDamageModifierDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        // Examples: "1d6", "1d4", "2d8", ...
        public string DamageDice { get; set; }

        [Required]
        // Examples: "1d6", "1d4", "2d8", ...
        public string CriticalDamageDice { get; set; }

        [Required]
        // Examples: "fire", "slashing", "piercing", ...
        public string Type { get; set; }

        [Required]
        public bool IsIncluded { get; set; }
    }
}
