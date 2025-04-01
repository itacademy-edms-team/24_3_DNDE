using System;
using System.Collections.Generic;

namespace DND5E_CE.Server.Models;

public partial class Character
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Race { get; set; }

    public string? Subrace { get; set; }

    public int? ClassId { get; set; }

    public string? Subclass { get; set; }

    public string? CreatureType { get; set; }

    public int? Level { get; set; }

    public int? Experience { get; set; }

    public int? ExperienceToNextLevelId { get; set; }

    public bool? Inspiration { get; set; }

    public int? Initiative { get; set; }

    public int? ProficiencyBonus { get; set; }

    public int? HitPointsCurrent { get; set; }

    public int? HitPointsMax { get; set; }

    public int? HitPointsTemp { get; set; }

    public int? DeathSaveSuccess { get; set; }

    public int? DeathSaveFail { get; set; }

    public int? SpeedBase { get; set; }

    public int? SpellSaveDc { get; set; }

    public string? SpellCastingAbility { get; set; }

    public int? SpellAttackBonus { get; set; }

    public int? Cp { get; set; }

    public int? Sp { get; set; }

    public int? Ep { get; set; }

    public int? Gp { get; set; }

    public int? Pp { get; set; }

    public string? PersonalityTraits { get; set; }

    public string? Ideals { get; set; }

    public string? Bonds { get; set; }

    public string? Flaws { get; set; }

    public virtual Ability? Ability { get; set; }

    public virtual AttackAndSpellcasting? AttackAndSpellcasting { get; set; }

    public virtual Bio? Bio { get; set; }

    public virtual CharacterClass? Class { get; set; }

    public virtual ClassResource? ClassResource { get; set; }

    public virtual ExperienceToNextLevel? ExperienceToNextLevel { get; set; }

    public virtual GlobalDamageModifier? GlobalDamageModifier { get; set; }

    public virtual OtherResource? OtherResource { get; set; }

    public virtual Skill? Skill { get; set; }

    public virtual Spell? Spell { get; set; }

    public virtual ToolOrCustomSkillProficiency? ToolOrCustomSkillProficiency { get; set; }
}
