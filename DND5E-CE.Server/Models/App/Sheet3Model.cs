using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DND5E_CE.Server.Models.App
{
    public class Sheet3Model
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

        // Header fields

        public string SpellBondAbility { get; set; } = String.Empty;

        // Body fields

        public int RemainingSpellSlotsLevel1 { get; set; } = 0;

        public int RemainingSpellSlotsLevel2 { get; set; } = 0;

        public int RemainingSpellSlotsLevel3 { get; set; } = 0;

        public int RemainingSpellSlotsLevel4 { get; set; } = 0;

        public int RemainingSpellSlotsLevel5 { get; set; } = 0;

        public int RemainingSpellSlotsLevel6 { get; set; } = 0;

        public int RemainingSpellSlotsLevel7 { get; set; } = 0;

        public int RemainingSpellSlotsLevel8 { get; set; } = 0;

        public int RemainingSpellSlotsLevel9 { get; set; } = 0;

    }
}
