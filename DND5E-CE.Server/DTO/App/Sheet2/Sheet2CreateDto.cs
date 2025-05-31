using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App.Sheet2
{
    public class Sheet2CreateDto
    {
        [Required]
        public string Age { get; set; }

        [Required]
        public string Height { get; set; }

        [Required]
        public string Weight { get; set; }

        [Required]
        public string Eyes { get; set; }

        [Required]
        public string Skin { get; set; }

        [Required]
        public string Hair { get; set; }

        [StringLength(1000)]
        public string Appearance { get; set; }

        [StringLength(1000)]
        public string Backstory { get; set; }

        [StringLength(1000)]
        public string AlliesAndOrganizations { get; set; }

        [StringLength(1000)]
        public string AdditionalFeaturesAndTraits { get; set; }

        [StringLength(1000)]
        public string Treasures { get; set; }
    }
}
