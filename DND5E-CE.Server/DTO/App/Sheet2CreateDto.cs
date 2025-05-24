using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App
{
    public class Sheet2CreateDto
    {
        public string Age { get; set; }

        
        public string Height { get; set; }

        
        public string Weight { get; set; }

        
        public string Eyes { get; set; }

        
        public string Skin { get; set; }

        
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
