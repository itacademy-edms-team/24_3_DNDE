using DND5E_CE.Server.Models.Enums;
using System;
using System.Collections.Generic;

namespace DND5E_CE.Server.Models;

public partial class ToolOrCustomSkillProficiency
{
    public int CharacterId { get; set; }

    public string? Name { get; set; }

    public int? ProficiencyLevel { get; set; }

    public CharacterAttribute Attribute { get; set; }

    public int Id { get; set; }

    public virtual Character IdNavigation { get; set; } = null!;
}
