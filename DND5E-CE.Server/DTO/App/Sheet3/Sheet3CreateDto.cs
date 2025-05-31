using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App.Sheet3
{
    public class Sheet3CreateDto
    {
        [Required]
        public string SpellBondAbility { get; set; }

        [Required]
        public int RemainingSpellSlotsLevel1 { get; set; }
        
        [Required]
        public int RemainingSpellSlotsLevel2 { get; set; }
        
        [Required]
        public int RemainingSpellSlotsLevel3 { get; set; }
        
        [Required]
        public int RemainingSpellSlotsLevel4 { get; set; }
        
        [Required]
        public int RemainingSpellSlotsLevel5 { get; set; }
        
        [Required]
        public int RemainingSpellSlotsLevel6 { get; set; }
        
        [Required]
        public int RemainingSpellSlotsLevel7 { get; set; }
        
        [Required]
        public int RemainingSpellSlotsLevel8 { get; set; }
        
        [Required]
        public int RemainingSpellSlotsLevel9 { get; set; }
    }
}
