using System;
using System.Collections.Generic;

namespace DND5E_CE.Server.Models;

public partial class OtherResource
{
    public int CharacterId { get; set; }

    public int? Total { get; set; }

    public int? Value { get; set; }

    public string? Name { get; set; }

    public virtual Character Character { get; set; } = null!;
}
