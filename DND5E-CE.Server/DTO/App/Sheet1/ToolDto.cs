using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App.Sheet1
{
    public class ToolDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ProficiencyType { get; set; } // "Proficient" | "Expertise" | "JackOfAllTrades"

        [Required]
        public string BondAbility { get; set; }

        [Required]
        public int Mods { get; set; }
    }
}
