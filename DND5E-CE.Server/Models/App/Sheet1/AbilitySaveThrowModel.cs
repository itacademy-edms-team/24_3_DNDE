using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DND5E_CE.Server.Models.App.Sheet1
{
    public class AbilitySaveThrowModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid Sheet1Id { get; set; }

        [ForeignKey("Sheet1Id")]
        public Sheet1Model Sheet1 { get; set; }

        [Required]
        public bool IsSaveThrowStrengthProficient { get; set; } = false;

        [Required]
        public bool IsSaveThrowDexterityProficient { get; set; } = false;

        [Required]
        public bool IsSaveThrowConstitutionProficient { get; set; } = false;

        [Required]
        public bool IsSaveThrowIntelligenceProficient { get; set; } = false;

        [Required]
        public bool IsSaveThrowWisdomProficient { get; set; } = false;

        [Required]
        public bool IsSaveThrowCharismaProficient { get; set; } = false;
    }
}
