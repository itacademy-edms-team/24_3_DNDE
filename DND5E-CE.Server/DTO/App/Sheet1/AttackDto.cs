using DND5E_CE.Server.Models.App.Sheet1;
using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App.Sheet1
{
    public class AttackDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        // AttackInfo fields
        [Required]
        public bool AtkIsIncluded { get; set; }

        [Required]
        // Possible values: "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma", "spell", "-"
        public string AtkBondAbility { get; set; }

        [Required]
        public int AtkBonus { get; set; }

        [Required]
        public bool AtkIsProficient { get; set; }

        [Required]
        public string AtkRange { get; set; }

        [Required]
        public int AtkMagicBonus { get; set; }

        [Required]
        public string AtkCritRange { get; set; }

        // Damage1 fields
        [Required]
        public bool Damage1IsIncluded { get; set; }

        [Required]
        public string Damage1Dice { get; set; }

        [Required]
        // Possible values: "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma", "spell", "-"
        public string Damage1BondAbility { get; set; }

        [Required]
        public int Damage1Bonus { get; set; }

        [Required]
        public string Damage1Type { get; set; }

        [Required]
        public string Damage1CritDice { get; set; }

        // Damage2 fields
        [Required]
        public bool Damage2IsIncluded { get; set; }

        [Required]
        public string Damage2Dice { get; set; }

        [Required]
        // Possible values: "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma", "spell", "-"
        public string Damage2BondAbility { get; set; }

        [Required]
        public int Damage2Bonus { get; set; }

        [Required]
        public string Damage2Type { get; set; }

        [Required]
        public string Damage2CritDice { get; set; }

        // AttackSavingThrow fields
        [Required]
        public bool SavingThrowIsIncluded { get; set; }

        [Required]
        // Possible values: "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma"
        public string SavingThrowBondAbility { get; set; }

        [Required]
        // Possible values: "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma", "spell", "flat"
        public string SavingThrowDifficultyClass { get; set; }

        [Required]
        public string SaveEffect { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
