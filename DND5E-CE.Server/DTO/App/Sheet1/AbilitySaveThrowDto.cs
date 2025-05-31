using DND5E_CE.Server.Models.App.Sheet1;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App.Sheet1
{
    public class AbilitySaveThrowDto
    {
        [Required]
        public bool IsSaveThrowStrengthProficient { get; set; }

        [Required]
        public bool IsSaveThrowDexterityProficient { get; set; }

        [Required]
        public bool IsSaveThrowConstitutionProficient { get; set; }

        [Required]
        public bool IsSaveThrowIntelligenceProficient { get; set; }

        [Required]
        public bool IsSaveThrowWisdomProficient { get; set; }

        [Required]
        public bool IsSaveThrowCharismaProficient { get; set; }
    }
}
