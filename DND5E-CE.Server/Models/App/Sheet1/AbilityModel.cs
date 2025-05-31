using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DND5E_CE.Server.Models.App.Sheet1
{
    public class AbilityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid Sheet1Id { get; set; }

        [ForeignKey("Sheet1Id")]
        public Sheet1Model Sheet1 { get; set; }

        [Required]
        public int StrengthBase { get; set; } = 15;

        [Required]
        public int DexterityBase { get; set; } = 14;

        [Required]
        public int ConstitutionBase { get; set; } = 13;

        [Required]
        public int IntelligenceBase { get; set; } = 12;

        [Required]
        public int WisdomBase { get; set; } = 10;

        [Required]
        public int CharismaBase { get; set; } = 8;
    }
}
