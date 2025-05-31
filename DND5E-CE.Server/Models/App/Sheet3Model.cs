using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DND5E_CE.Server.Models.App
{
    public class Sheet3Model
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CharacterId { get; set; }

        [ForeignKey("CharacterId")]
        public CharacterModel Character { get; set; }

        // Header fields
        [Required]
        public string SpellBondAbility { get; set; } = String.Empty;

        // Body fields
        [Required]
        public int RemainingSpellSlotsLevel1 { get; set; } = 0;

        [Required]
        public int RemainingSpellSlotsLevel2 { get; set; } = 0;

        [Required]
        public int RemainingSpellSlotsLevel3 { get; set; } = 0;

        [Required]
        public int RemainingSpellSlotsLevel4 { get; set; } = 0;

        [Required]
        public int RemainingSpellSlotsLevel5 { get; set; } = 0;

        [Required]
        public int RemainingSpellSlotsLevel6 { get; set; } = 0;

        [Required]
        public int RemainingSpellSlotsLevel7 { get; set; } = 0;

        [Required]
        public int RemainingSpellSlotsLevel8 { get; set; } = 0;

        [Required]
        public int RemainingSpellSlotsLevel9 { get; set; } = 0;

    }
}
