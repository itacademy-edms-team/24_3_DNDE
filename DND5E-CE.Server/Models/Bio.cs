using System;
using System.Collections.Generic;

namespace DND5E_CE.Server.Models;

public partial class Bio
{
    public int CharacterId { get; set; }

    public string? CharacterAppearance { get; set; }

    public string? Backstory { get; set; }

    public string? AlliesAndOrganisations { get; set; }

    public string? AdditionalFeatureAndTraits { get; set; }

    public string? Treasure { get; set; }

    public virtual Character Character { get; set; } = null!;
}
