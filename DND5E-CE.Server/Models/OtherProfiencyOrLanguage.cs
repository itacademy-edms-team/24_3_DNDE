using System;
using System.Collections.Generic;

namespace DND5E_CE.Server.Models;

public partial class OtherProfiencyOrLanguage
{
    public int Id { get; set; }

    public int CharacterId { get; set; }

    public OtherProfiencyOrLanguage? Type { get; set; }

    public string? Profiency { get; set; }

    public virtual Character? Character { get; set; }
}
