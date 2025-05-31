using DND5E_CE.Server.Models.App.Sheet1;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App.Sheet1
{
    public class AbilityDto
    {
        [Required]
        public int StrengthBase { get; set; }

        [Required]
        public int DexterityBase { get; set; }

        [Required]
        public int ConstitutionBase { get; set; }

        [Required]
        public int IntelligenceBase { get; set; }

        [Required]
        public int WisdomBase { get; set; }

        [Required]
        public int CharismaBase { get; set; }
    }
}
