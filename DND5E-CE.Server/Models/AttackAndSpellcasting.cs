using System;
using System.Collections.Generic;

namespace DND5E_CE.Server.Models;

public enum CharacterAbilityShort
{
  STR,
  DEX,
  CON,
  INT,
  WIS,
  CHA,
  None
}

public partial class AttackAndSpellcasting
{
    public int CharacterId { get; set; }

    public string? Name { get; set; }

    public string? Range { get; set; }

    public int? MagicBonus { get; set; }

    public bool? IsProficient { get; set; }

    public int? CritRange { get; set; }

    public string? Damage1Dice { get; set; }

    public CharacterAbilityShort Damage1Ability { get; set; }

    public int? Damage1Bonus { get; set; }

    public string? Damage1CritDice { get; set; }

    public virtual Character Character { get; set; } = null!;
}
