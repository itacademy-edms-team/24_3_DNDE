using System;
using System.Collections.Generic;

namespace DND5E_CE.Server.Models;

public partial class Spell
{
    public int CharacterId { get; set; }

    public int? Level { get; set; }

    public string? Name { get; set; }

    public string? School { get; set; }

    public bool? IsRitual { get; set; }

    public string? CastingTime { get; set; }

    public string? Range { get; set; }

    public string? Target { get; set; }

    public bool? VComponent { get; set; }

    public bool? SComponent { get; set; }

    public bool? MComponent { get; set; }

    public string? ComponentDescription { get; set; }

    public bool? IsConcentration { get; set; }

    public string? Duration { get; set; }

    public string? SpellCastingAbility { get; set; }

    public string? Innate { get; set; }

    public string? Output { get; set; }

    public string? Description { get; set; }

    public string? AtHigherLevelsDescription { get; set; }

    public string? Class { get; set; }

    public string? Type { get; set; }

    public int Id { get; set; }

    public virtual Character Character { get; set; } = null!;
}
