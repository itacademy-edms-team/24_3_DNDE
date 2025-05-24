using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DND5E_CE.Server.Models.App
{
    public class Sheet2Model
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

        // Header fields
        public string Age { get; set; } = String.Empty;

        public string Height { get; set; } = String.Empty;

        public string Weight { get; set; } = String.Empty;

        public string Eyes { get; set; } = String.Empty;

        public string Skin { get; set; } = String.Empty;

        public string Hair { get; set; } = String.Empty;

        // Body fields

        [StringLength(1000)]
        public string Appearance { get; set; } = String.Empty;

        [StringLength(1000)]
        public string Backstory { get; set; } = String.Empty;

        [StringLength(1000)]
        public string AlliesAndOrganizations { get; set; } = String.Empty;

        [StringLength(1000)]
        public string AdditionalFeaturesAndTraits { get; set; } = String.Empty;

        [StringLength(1000)]
        public string Treasures { get; set; } = String.Empty;

    }
}
