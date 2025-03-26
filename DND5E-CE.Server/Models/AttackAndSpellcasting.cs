using System;
using System.Collections.Generic;
using DND5E_CE.Server.Models.Enums;

namespace DND5E_CE.Server.Models;

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

    public int Id { get; set; }

    public virtual Character Character { get; set; } = null!;
}
