using System;
using System.Collections.Generic;

namespace DND5E_CE.Server.Models;

public partial class GlobalDamageModifier
{
    public int CharacterId { get; set; }

    public string? Name { get; set; }

    public string? DamageDice { get; set; }

    public string? CriticalDamageDice { get; set; }

    public string? Type { get; set; }

    public virtual Character Character { get; set; } = null!;
}
