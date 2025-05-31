using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.Models.App.Sheet1
{
    public class AttackModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid Sheet1Id { get; set; }

        [Required]
        public Sheet1Model Sheet1 { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        // AttackInfo fields
        [Required]
        public bool AtkIsIncluded { get; set; } = true;

        [Required]
        // Possible values: "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma", "spell", "-"
        public string AtkBondAbility { get; set; } = string.Empty;

        [Required]
        public int AtkBonus { get; set; } = 0;

        [Required]
        public bool AtkIsProficient { get; set; } = false;

        [Required]
        public string AtkRange { get; set; } = string.Empty;

        [Required]
        public int AtkMagicBonus { get; set; } = 0;

        [Required]
        public string AtkCritRange { get; set; } = string.Empty;

        // Damage1 fields
        [Required]
        public bool Damage1IsIncluded { get; set; } = true;

        [Required]
        public string Damage1Dice { get; set; } = string.Empty;

        [Required]
        // Possible values: "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma", "spell", "-"
        public string Damage1BondAbility { get; set; } = string.Empty;

        [Required]
        public int Damage1Bonus { get; set; } = 0;

        [Required]
        public string Damage1Type { get; set; } = string.Empty;

        [Required]
        public string Damage1CritDice { get; set; } = string.Empty;

        // Damage2 fields
        [Required]
        public bool Damage2IsIncluded { get; set; } = false;

        [Required]
        public string Damage2Dice { get; set; } = string.Empty;

        [Required]
        // Possible values: "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma", "spell", "-"
        public string Damage2BondAbility { get; set; } = string.Empty;

        [Required]
        public int Damage2Bonus { get; set; } = 0;

        [Required]
        public string Damage2Type { get; set; } = string.Empty;

        [Required]
        public string Damage2CritDice { get; set; } = string.Empty;

        // AttackSavingThrow fields
        [Required]
        public bool SavingThrowIsIncluded { get; set; } = false;

        [Required]
        // Possible values: "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma"
        public string SavingThrowBondAbility { get; set; } = string.Empty;

        [Required]
        // Possible values: "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma", "spell", "flat"
        public string SavingThrowDifficultyClass { get; set; } = string.Empty;

        [Required]
        public string SaveEffect { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;
    }
}
