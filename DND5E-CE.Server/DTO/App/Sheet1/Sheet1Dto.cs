using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App.Sheet1
{
    public class Sheet1Dto
    {
        // Header fields
        [Required]
        public string Name { get; set; }

        [Required]
        public string Class { get; set; }

        [Required]
        public int Level { get; set; }

        [Required]
        public string Race { get; set; }

        [Required]
        public string Backstory { get; set; }

        [Required]
        public string Worldview { get; set; }

        [Required]
        public string PlayerName { get; set; }

        [Required]
        public int Experience { get; set; }

        // Body fields

        [Required]
        public AbilityDto Ability { get; set; }

        [Required]
        public AbilitySaveThrowDto AbilitySaveThrow { get; set; }

        [Required]
        public SkillDto Skill { get; set; }

        [Required]
        public ICollection<ToolDto> Tool { get; set; }

        [Required]
        public ICollection<OtherToolDto> OtherTool { get; set; }

        [Required]
        public int ArmorClass { get; set; }

        [Required]
        public int Speed { get; set; }

        [Required]
        public int Initiative { get; set; }

        [Required]
        public HitPointDto HitPoint { get; set; }

        [Required]
        public HitDiceDto HitDice { get; set; }

        [Required]
        public DeathSaveThrowDto DeathSaveThrow { get; set; }

        [Required]
        public ICollection<AttackDto> Attack { get; set; }

        [Required]
        public ICollection<GlobalDamageModifierDto> GlobalDamageModifier { get; set; }

        [Required]
        public InventoryGoldDto InventoryGold { get; set; }

        [Required]
        public ICollection<InventoryItemDto> InventoryItem { get; set; }

        [Required]
        public string PersonalityTraits { get; set; }

        [Required]
        public string Ideals { get; set; }

        [Required]
        public string Bonds { get; set; }

        [Required]
        public string Flaws { get; set; }

        [Required]
        public ClassResourceDto ClassResource { get; set; }

        [Required]
        public OtherResourceDto[] OtherResource { get; set; }
    }
}
