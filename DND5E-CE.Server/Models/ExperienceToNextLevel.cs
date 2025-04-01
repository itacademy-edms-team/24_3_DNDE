using System;
using System.Collections.Generic;

namespace DND5E_CE.Server.Models;

public partial class ExperienceToNextLevel
{
    public int Level { get; set; }

    public int? Experience { get; set; }

    public virtual ICollection<Character> Characters { get; set; } = new List<Character>();
}
