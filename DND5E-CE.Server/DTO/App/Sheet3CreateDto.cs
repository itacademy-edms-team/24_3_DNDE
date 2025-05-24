using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App
{
    public class Sheet3CreateDto
    {
        public string SpellBondAbility { get; set; }

        public int RemainingSpellSlotsLevel1 { get; set; }
        public int RemainingSpellSlotsLevel2 { get; set; }
        public int RemainingSpellSlotsLevel3 { get; set; }
        public int RemainingSpellSlotsLevel4 { get; set; }
        public int RemainingSpellSlotsLevel5 { get; set; }
        public int RemainingSpellSlotsLevel6 { get; set; }
        public int RemainingSpellSlotsLevel7 { get; set; }
        public int RemainingSpellSlotsLevel8 { get; set; }
        public int RemainingSpellSlotsLevel9 { get; set; }
    }
}
