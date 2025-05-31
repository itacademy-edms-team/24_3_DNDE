using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DND5E_CE.Server.Models.App
{
    public class Sheet2Model
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CharacterId { get; set; }

        [ForeignKey("CharacterId")]
        public CharacterModel Character { get; set; }

        // Header fields
        [Required]
        public string Age { get; set; } = String.Empty;

        [Required]
        public string Height { get; set; } = String.Empty;

        [Required]
        public string Weight { get; set; } = String.Empty;

        [Required]
        public string Eyes { get; set; } = String.Empty;

        [Required]
        public string Skin { get; set; } = String.Empty;

        [Required]
        public string Hair { get; set; } = String.Empty;

        // Body fields
        [Required]
        [StringLength(1000)]
        public string Appearance { get; set; } = String.Empty;

        [Required]
        [StringLength(1000)]
        public string Backstory { get; set; } = String.Empty;

        [Required]
        [StringLength(1000)]
        public string AlliesAndOrganizations { get; set; } = String.Empty;

        [Required]
        [StringLength(1000)]
        public string AdditionalFeaturesAndTraits { get; set; } = String.Empty;

        [Required]
        [StringLength(1000)]
        public string Treasures { get; set; } = String.Empty;

    }
}
