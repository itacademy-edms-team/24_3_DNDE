using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DND5E_CE.Server.Models.App.Sheet1
{
    public class Sheet1Model
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CharacterId { get; set; }

        [ForeignKey("CharacterId")]
        public CharacterModel Character { get; set; }

        // Header fields

        [Required]
        public string Name { get; set; } = "Новый персонаж";

        [Required]
        public string Class { get; set; } = string.Empty;

        [Required]
        public int Level { get; set; } = 1;

        [Required]
        public string Race { get; set; } = string.Empty;

        [Required]
        public string Backstory { get; set; } = string.Empty;

        [Required]
        public string Worldview { get; set; } = string.Empty;

        [Required]
        public string PlayerName { get; set; } = string.Empty;

        [Required]
        public int Experience { get; set; } = 0;

        // Body fields

        // Abilities
        [Required]
        public Guid AbilityId { get; set; }

        public AbilityModel Ability { get; set; }

        // Ability save throws
        [Required]
        public Guid AbilitySaveThrowId { get; set; }

        public AbilitySaveThrowModel AbilitySaveThrow { get; set; }

        // Skills
        [Required]
        public Guid SkillId { get; set; }

        public SkillModel Skill { get; set; }

        // Tools
        public ICollection<ToolModel> Tool { get; set; } = new List<ToolModel>();

        // Other Tools
        public ICollection<OtherToolModel> OtherTool { get; set; } = new List<OtherToolModel>();

        // Armor Class
        [Required]
        public int ArmorClass { get; set; } = 10;

        [Required]
        public int Initiative {  get; set; } = 0;

        [Required]
        public int Speed { get; set; } = 10;

        // Hit points
        [Required]
        public Guid HitPointId { get; set; }

        public HitPointModel HitPoint { get; set; }

        // Hit dice
        [Required]
        public Guid HitDiceId { get; set; }

        public HitDiceModel HitDice { get; set; }

        // Death save throws
        [Required]
        public Guid DeathSaveThrowId { get; set; }

        public DeathSaveThrowModel DeathSaveThrow { get; set; }

        // Attacks
        public ICollection<AttackModel> Attack { get; set; } = new List<AttackModel>();

        // Global damage modifiers
        public ICollection<GlobalDamageModifierModel> GlobalDamageModifier { get; set; } = new List<GlobalDamageModifierModel>();

        // Inventory gold
        [Required]
        public Guid InventoryGoldId { get; set; }
        public InventoryGoldModel InventoryGold { get; set; }

        // Inventory items
        public ICollection<InventoryItemModel> InventoryItem {  get; set; } = new List<InventoryItemModel>();

        // Personality traits, ideals, bonds, flaws
        [Required]
        public string PersonalityTraits { get; set; } = string.Empty;

        [Required]
        public string Ideals { get; set; } = string.Empty;

        [Required]
        public string Bonds { get; set; } = string.Empty;

        [Required]
        public string Flaws { get; set; } = string.Empty;

        // Class resource
        [Required]
        public Guid ClassResourceId { get; set; }
        public ClassResourceModel ClassResource { get; set; }

        // Other resources
        public ICollection<OtherResourceModel> OtherResource { get; set; } = new List<OtherResourceModel>();
    }
}
