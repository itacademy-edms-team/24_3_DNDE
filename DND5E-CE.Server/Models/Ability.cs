using System;
using System.Collections.Generic;

namespace DND5E_CE.Server.Models;

public partial class Ability
{
    public int CharacterId { get; set; }

    public int? Strength { get; set; }

    public int? StrengthBase { get; set; }

    public int? StrengthBonus { get; set; }

    public int? Dexterity { get; set; }

    public int? DexterityBase { get; set; }

    public int? DexterityBonus { get; set; }

    public int? Constitution { get; set; }

    public int? ConstitutionBase { get; set; }

    public int? ConstitutionBonus { get; set; }

    public int? Intelligence { get; set; }

    public int? IntelligenceBase { get; set; }

    public int? IntelligenceBonus { get; set; }

    public int? Wisdom { get; set; }

    public int? WisdomBase { get; set; }

    public int? WisdomBonus { get; set; }

    public int? Charisma { get; set; }

    public int? CharismaBase { get; set; }

    public int? CharismaBonus { get; set; }

    public virtual Character Character { get; set; } = null!;
}
