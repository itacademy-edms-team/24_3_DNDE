using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App.Sheet2
{
    public class Sheet2Dto
    {
        // Header fields
        public string Age { get; set; }

        public string Height { get; set; }

        public string Weight { get; set; }

        public string Eyes { get; set; }

        public string Skin { get; set; }

        public string Hair { get; set; }

        // Body fields

        
        public string Appearance { get; set; }

        
        public string Backstory { get; set; }

        
        public string AlliesAndOrganizations { get; set; }

        public string AdditionalFeaturesAndTraits { get; set; }

        public string Treasures { get; set; }
    }
}
