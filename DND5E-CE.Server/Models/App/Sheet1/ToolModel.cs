using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DND5E_CE.Server.Models.App.Sheet1
{
    public class ToolModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid Sheet1Id { get; set; }

        [ForeignKey("Sheet1Id")]
        public Sheet1Model Sheet1 { get; set; }

        [Required]
        public string Name { get; set; } = "Новый инструмент";

        [Required]
        public string ProficiencyType { get; set; } = "Proficient"; // "Proficient" | "Expertise" | "JackOfAllTrades"

        [Required]
        public string BondAbility { get; set; } = "Strength";

        [Required]
        public int Mods { get; set; } = 0;
    }
}
