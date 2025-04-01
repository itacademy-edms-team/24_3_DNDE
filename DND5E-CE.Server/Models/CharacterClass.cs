using System;
using System.Collections.Generic;

namespace DND5E_CE.Server.Models;

public partial class CharacterClass
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Source { get; set; }

    public virtual ICollection<Character> Characters { get; set; } = new List<Character>();
}
