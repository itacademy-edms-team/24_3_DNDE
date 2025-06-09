using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App.Sheet3
{
    public class SpellDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public int Level { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string School { get; set; } = null!;

        [Required]
        public bool IsRitual { get; set; }

        [Required]
        public string CastingTime { get; set; } = null!;

        [Required]
        public string Range { get; set; } = null!;

        [Required]
        public string Target { get; set; } = null!;

        [Required]
        public bool ComponentsV { get; set; }

        [Required]
        public bool ComponentsS { get; set; }

        [Required]
        public bool ComponentsM { get; set; }

        [Required]
        public string ComponentsDescription { get; set; } = null!;

        [Required]
        public bool IsConcentration { get; set; }

        [Required]
        public string Duration { get; set; } = null!;

        [Required]
        public string SpellCastingAbility { get; set; } = null!;

        [Required]
        public string Innate { get; set; } = null!;

        [Required]
        public string Output { get; set; } = null!;

        [Required]
        public string AttackType { get; set; } = null!;

        [Required]
        public string AttackDamage1Dice { get; set; } = null!;

        [Required]
        public string AttackDamage1Type { get; set; } = null!;

        [Required]
        public string AttackDamage2Dice { get; set; } = null!;

        [Required]
        public string AttackDamage2Type { get; set; } = null!;

        [Required]
        public string HealingDice { get; set; } = null!;

        [Required]
        public bool IsAbilityModIncluded { get; set; }

        [Required]
        public string SavingThrowAbility { get; set; } = null!;

        [Required]
        public string SavingThrowEffect { get; set; } = null!;

        [Required]
        public int HigherLevelCastDiceAmount { get; set; }

        [Required]
        public string HigherLevelCastDiceType { get; set; } = null!;

        [Required]
        public int HigherLevelCastBonus { get; set; }

        [Required]
        public string IncludeSpellDescriptionInAttack { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public string AtHigherLevels { get; set; } = null!;

        [Required]
        public string Class { get; set; } = null!;

        [Required]
        public string Type { get; set; } = null!;

        [Required]
        public string CantripProgression { get; set; } = null!;

        [Required]
        public bool IsPrepared { get; set; }
    }
}
