using System;
using System.Collections.Generic;
using DND5E_CE.Server.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DND5E_CE.Server.Models;

public partial class DND5EContext : DbContext
{
    public DND5EContext()
    {
    }

    public DND5EContext(DbContextOptions<DND5EContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Ability> Abilities { get; set; }

    public virtual DbSet<AttackAndSpellcasting> AttackAndSpellcastings { get; set; }

    public virtual DbSet<Bio> Bios { get; set; }

    public virtual DbSet<Character> Characters { get; set; }

    public virtual DbSet<CharacterClass> CharacterClasses { get; set; }

    public virtual DbSet<ClassResource> ClassResources { get; set; }

    public virtual DbSet<ExperienceToNextLevel> ExperienceToNextLevels { get; set; }

    public virtual DbSet<GlobalDamageModifier> GlobalDamageModifiers { get; set; }

    public virtual DbSet<OtherProfiencyOrLanguage> OtherProfiencyOrLanguages { get; set; }

    public virtual DbSet<OtherResource> OtherResources { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<Spell> Spells { get; set; }

    public virtual DbSet<ToolOrCustomSkillProficiency> ToolOrCustomSkillProficiencies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("character_ability_short", new[] { "STR", "DEX", "CON", "INT", "WIS", "CHA", "-" })
            .HasPostgresEnum("character_attribute", new[] { "strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma" })
            .HasPostgresEnum("other_profiency_type", new[] { "armor", "language", "weapon", "other" });

        modelBuilder.Entity<Ability>(entity =>
        {
            entity.HasKey(e => e.CharacterId).HasName("ability_pkey");

            entity.ToTable("ability");

            entity.Property(e => e.CharacterId)
                .ValueGeneratedNever()
                .HasColumnName("character_id");
            entity.Property(e => e.Charisma)
                .HasDefaultValue(0)
                .HasColumnName("charisma");
            entity.Property(e => e.CharismaBase)
                .HasDefaultValue(0)
                .HasColumnName("charisma_base");
            entity.Property(e => e.CharismaBonus)
                .HasDefaultValue(0)
                .HasColumnName("charisma_bonus");
            entity.Property(e => e.Constitution)
                .HasDefaultValue(0)
                .HasColumnName("constitution");
            entity.Property(e => e.ConstitutionBase)
                .HasDefaultValue(0)
                .HasColumnName("constitution_base");
            entity.Property(e => e.ConstitutionBonus)
                .HasDefaultValue(0)
                .HasColumnName("constitution_bonus");
            entity.Property(e => e.Dexterity)
                .HasDefaultValue(0)
                .HasColumnName("dexterity");
            entity.Property(e => e.DexterityBase)
                .HasDefaultValue(0)
                .HasColumnName("dexterity_base");
            entity.Property(e => e.DexterityBonus)
                .HasDefaultValue(0)
                .HasColumnName("dexterity_bonus");
            entity.Property(e => e.Intelligence)
                .HasDefaultValue(0)
                .HasColumnName("intelligence");
            entity.Property(e => e.IntelligenceBase)
                .HasDefaultValue(0)
                .HasColumnName("intelligence_base");
            entity.Property(e => e.IntelligenceBonus)
                .HasDefaultValue(0)
                .HasColumnName("intelligence_bonus");
            entity.Property(e => e.Strength)
                .HasDefaultValue(0)
                .HasColumnName("strength");
            entity.Property(e => e.StrengthBase)
                .HasDefaultValue(0)
                .HasColumnName("strength_base");
            entity.Property(e => e.StrengthBonus)
                .HasDefaultValue(0)
                .HasColumnName("strength_bonus");
            entity.Property(e => e.Wisdom)
                .HasDefaultValue(0)
                .HasColumnName("wisdom");
            entity.Property(e => e.WisdomBase)
                .HasDefaultValue(0)
                .HasColumnName("wisdom_base");
            entity.Property(e => e.WisdomBonus)
                .HasDefaultValue(0)
                .HasColumnName("wisdom_bonus");

            entity.HasOne(d => d.Character).WithOne(p => p.Ability)
                .HasForeignKey<Ability>(d => d.CharacterId)
                .HasConstraintName("fk_ability_character");
        });

        modelBuilder.Entity<AttackAndSpellcasting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("attack_and_spellcasting_pkey");

            entity.ToTable("attack_and_spellcasting");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CharacterId).HasColumnName("character_id");
            entity.Property(e => e.CritRange)
                .HasDefaultValue(0)
                .HasColumnName("crit_range");
            entity.Property(e => e.Damage1Bonus)
                .HasDefaultValue(0)
                .HasColumnName("damage1_bonus");
            entity.Property(e => e.Damage1CritDice)
                .HasMaxLength(255)
                .HasDefaultValueSql("'1d6'::character varying")
                .HasColumnName("damage1_crit_dice");
            entity.Property(e => e.Damage1Dice)
                .HasMaxLength(255)
                .HasDefaultValueSql("'1d6'::character varying")
                .HasColumnName("damage1_dice");
            entity.Property(e => e.Damage1Ability)
                .HasConversion(
                    v => v.ToString(),
                    v => (CharacterAbilityShort)Enum.Parse(typeof(CharacterAbilityShort), v));
            entity.Property(e => e.IsProficient)
                .HasDefaultValue(false)
                .HasColumnName("is_proficient");
            entity.Property(e => e.MagicBonus)
                .HasDefaultValue(0)
                .HasColumnName("magic_bonus");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Range)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("range");

            entity.HasOne(d => d.Character).WithMany(p => p.AttackAndSpellcastings)
                .HasForeignKey(d => d.CharacterId)
                .HasConstraintName("fk_attack_and_spellcasting_character");
        });

        modelBuilder.Entity<Bio>(entity =>
        {
            entity.HasKey(e => e.CharacterId).HasName("bio_pkey");

            entity.ToTable("bio");

            entity.Property(e => e.CharacterId)
                .ValueGeneratedNever()
                .HasColumnName("character_id");
            entity.Property(e => e.AdditionalFeatureAndTraits).HasColumnName("additional_feature_and_traits");
            entity.Property(e => e.AlliesAndOrganisations).HasColumnName("allies_and_organisations");
            entity.Property(e => e.Backstory).HasColumnName("backstory");
            entity.Property(e => e.CharacterAppearance).HasColumnName("character_appearance");
            entity.Property(e => e.Treasure).HasColumnName("treasure");

            entity.HasOne(d => d.Character).WithOne(p => p.Bio)
                .HasForeignKey<Bio>(d => d.CharacterId)
                .HasConstraintName("fk_character_bio");
        });

        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("character_pkey");

            entity.ToTable("character");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bonds)
                .HasDefaultValueSql("''::text")
                .HasColumnName("bonds");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.Cp)
                .HasDefaultValue(0)
                .HasColumnName("cp");
            entity.Property(e => e.CreatureType)
                .HasMaxLength(255)
                .HasColumnName("creature_type");
            entity.Property(e => e.DeathSaveFail)
                .HasDefaultValue(0)
                .HasColumnName("death_save_fail");
            entity.Property(e => e.DeathSaveSuccess)
                .HasDefaultValue(0)
                .HasColumnName("death_save_success");
            entity.Property(e => e.Ep)
                .HasDefaultValue(0)
                .HasColumnName("ep");
            entity.Property(e => e.Experience)
                .HasDefaultValue(0)
                .HasColumnName("experience");
            entity.Property(e => e.Flaws)
                .HasDefaultValueSql("''::text")
                .HasColumnName("flaws");
            entity.Property(e => e.Gp)
                .HasDefaultValue(0)
                .HasColumnName("gp");
            entity.Property(e => e.HitPointsCurrent).HasColumnName("hit_points_current");
            entity.Property(e => e.HitPointsMax).HasColumnName("hit_points_max");
            entity.Property(e => e.HitPointsTemp)
                .HasDefaultValue(0)
                .HasColumnName("hit_points_temp");
            entity.Property(e => e.Ideals)
                .HasDefaultValueSql("''::text")
                .HasColumnName("ideals");
            entity.Property(e => e.Initiative).HasColumnName("initiative");
            entity.Property(e => e.Inspiration)
                .HasDefaultValue(false)
                .HasColumnName("inspiration");
            entity.Property(e => e.Level)
                .HasDefaultValue(1)
                .HasColumnName("level");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.PersonalityTraits)
                .HasDefaultValueSql("''::text")
                .HasColumnName("personality_traits");
            entity.Property(e => e.Pp)
                .HasDefaultValue(0)
                .HasColumnName("pp");
            entity.Property(e => e.ProficiencyBonus).HasColumnName("proficiency_bonus");
            entity.Property(e => e.Race)
                .HasMaxLength(255)
                .HasColumnName("race");
            entity.Property(e => e.Sp)
                .HasDefaultValue(0)
                .HasColumnName("sp");
            entity.Property(e => e.SpeedBase)
                .HasDefaultValue(0)
                .HasColumnName("speed_base");
            entity.Property(e => e.SpellAttackBonus).HasColumnName("spell_attack_bonus");
            entity.Property(e => e.SpellCastingAbility)
                .HasMaxLength(255)
                .HasColumnName("spell_casting_ability");
            entity.Property(e => e.SpellSaveDc)
                .HasDefaultValue(0)
                .HasColumnName("spell_save_dc");
            entity.Property(e => e.Subclass)
                .HasMaxLength(255)
                .HasColumnName("subclass");
            entity.Property(e => e.Subrace)
                .HasMaxLength(255)
                .HasColumnName("subrace");

            entity.HasOne(d => d.Class).WithMany(p => p.Characters)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("fk_character_character_class");
        });

        modelBuilder.Entity<CharacterClass>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("character_class_pkey");

            entity.ToTable("character_class");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Source)
                .HasMaxLength(255)
                .HasColumnName("source");
        });

        modelBuilder.Entity<ClassResource>(entity =>
        {
            entity.HasKey(e => e.CharacterId).HasName("class_resource_pkey");

            entity.ToTable("class_resource");

            entity.Property(e => e.CharacterId)
                .ValueGeneratedNever()
                .HasColumnName("character_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Total).HasColumnName("total");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.Character).WithOne(p => p.ClassResource)
                .HasForeignKey<ClassResource>(d => d.CharacterId)
                .HasConstraintName("fk_class_resource_character");
        });

        modelBuilder.Entity<ExperienceToNextLevel>(entity =>
        {
            entity.HasKey(e => e.Level).HasName("experience_to_next_level_pkey");

            entity.ToTable("experience_to_next_level");

            entity.Property(e => e.Level)
                .ValueGeneratedNever()
                .HasColumnName("level");
            entity.Property(e => e.Experience).HasColumnName("experience");
        });

        modelBuilder.Entity<GlobalDamageModifier>(entity =>
        {
            entity.HasKey(e => e.CharacterId).HasName("global_damage_modifier_pkey");

            entity.ToTable("global_damage_modifier");

            entity.Property(e => e.CharacterId)
                .ValueGeneratedNever()
                .HasColumnName("character_id");
            entity.Property(e => e.CriticalDamageDice)
                .HasMaxLength(255)
                .HasColumnName("critical_damage_dice");
            entity.Property(e => e.DamageDice)
                .HasMaxLength(255)
                .HasColumnName("damage_dice");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .HasColumnName("type");

            entity.HasOne(d => d.Character).WithOne(p => p.GlobalDamageModifier)
                .HasForeignKey<GlobalDamageModifier>(d => d.CharacterId)
                .HasConstraintName("fk_global_damage_modifier_character");
        });

        modelBuilder.Entity<OtherProfiencyOrLanguage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("other_profiency_or_language_pkey");

            entity.ToTable("other_profiency_or_language");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CharacterId).HasColumnName("character_id");
            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasConversion(
                    v => v.ToString(),
                    v => (OtherProfiencyOrLanguage)Enum.Parse(typeof(OtherProfiencyOrLanguage), v));
            entity.Property(e => e.Profiency)
                .HasMaxLength(50)
                .HasColumnName("profiency");

            entity.HasOne(d => d.Character).WithMany(p => p.OtherProfiencyOrLanguages)
                .HasForeignKey(d => d.CharacterId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_other_profiency_or_language_character");
        });

        modelBuilder.Entity<OtherResource>(entity =>
        {
            entity.HasKey(e => e.CharacterId).HasName("other_resource_pkey");

            entity.ToTable("other_resource");

            entity.Property(e => e.CharacterId)
                .ValueGeneratedNever()
                .HasColumnName("character_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Total).HasColumnName("total");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.Character).WithOne(p => p.OtherResource)
                .HasForeignKey<OtherResource>(d => d.CharacterId)
                .HasConstraintName("fk_other_resource_character");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.CharacterId).HasName("skill_pkey");

            entity.ToTable("skill");

            entity.Property(e => e.CharacterId)
                .ValueGeneratedNever()
                .HasColumnName("character_id");
            entity.Property(e => e.AcrobaticsBonus)
                .HasDefaultValue(0)
                .HasColumnName("acrobatics_bonus");
            entity.Property(e => e.AcrobaticsProficiency)
                .HasDefaultValue(0)
                .HasColumnName("acrobatics_proficiency");
            entity.Property(e => e.AcrobaticsType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("acrobatics_type");
            entity.Property(e => e.AnimalHandlingBonus)
                .HasDefaultValue(0)
                .HasColumnName("animal_handling_bonus");
            entity.Property(e => e.AnimalHandlingProficiency)
                .HasDefaultValue(0)
                .HasColumnName("animal_handling_proficiency");
            entity.Property(e => e.AnimalHandlingType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("animal_handling_type");
            entity.Property(e => e.ArcanaBonus)
                .HasDefaultValue(0)
                .HasColumnName("arcana_bonus");
            entity.Property(e => e.ArcanaProficiency)
                .HasDefaultValue(0)
                .HasColumnName("arcana_proficiency");
            entity.Property(e => e.ArcanaType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("arcana_type");
            entity.Property(e => e.AthleticsBonus)
                .HasDefaultValue(0)
                .HasColumnName("athletics_bonus");
            entity.Property(e => e.AthleticsProficiency)
                .HasDefaultValue(0)
                .HasColumnName("athletics_proficiency");
            entity.Property(e => e.AthleticsType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("athletics_type");
            entity.Property(e => e.DeceptionBonus)
                .HasDefaultValue(0)
                .HasColumnName("deception_bonus");
            entity.Property(e => e.DeceptionProficiency)
                .HasDefaultValue(0)
                .HasColumnName("deception_proficiency");
            entity.Property(e => e.DeceptionType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("deception_type");
            entity.Property(e => e.HistoryBonus)
                .HasDefaultValue(0)
                .HasColumnName("history_bonus");
            entity.Property(e => e.HistoryProficiency)
                .HasDefaultValue(0)
                .HasColumnName("history_proficiency");
            entity.Property(e => e.HistoryType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("history_type");
            entity.Property(e => e.InsightBonus)
                .HasDefaultValue(0)
                .HasColumnName("insight_bonus");
            entity.Property(e => e.InsightProficiency)
                .HasDefaultValue(0)
                .HasColumnName("insight_proficiency");
            entity.Property(e => e.InsightType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("insight_type");
            entity.Property(e => e.IntimidationBonus)
                .HasDefaultValue(0)
                .HasColumnName("intimidation_bonus");
            entity.Property(e => e.IntimidationProficiency)
                .HasDefaultValue(0)
                .HasColumnName("intimidation_proficiency");
            entity.Property(e => e.IntimidationType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("intimidation_type");
            entity.Property(e => e.InvestigationBonus)
                .HasDefaultValue(0)
                .HasColumnName("investigation_bonus");
            entity.Property(e => e.InvestigationProficiency)
                .HasDefaultValue(0)
                .HasColumnName("investigation_proficiency");
            entity.Property(e => e.InvestigationType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("investigation_type");
            entity.Property(e => e.MedicineBonus)
                .HasDefaultValue(0)
                .HasColumnName("medicine_bonus");
            entity.Property(e => e.MedicineProficiency)
                .HasDefaultValue(0)
                .HasColumnName("medicine_proficiency");
            entity.Property(e => e.MedicineType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("medicine_type");
            entity.Property(e => e.NatureBonus)
                .HasDefaultValue(0)
                .HasColumnName("nature_bonus");
            entity.Property(e => e.NatureProficiency)
                .HasDefaultValue(0)
                .HasColumnName("nature_proficiency");
            entity.Property(e => e.NatureType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("nature_type");
            entity.Property(e => e.PerceptionBonus)
                .HasDefaultValue(0)
                .HasColumnName("perception_bonus");
            entity.Property(e => e.PerceptionProficiency)
                .HasDefaultValue(0)
                .HasColumnName("perception_proficiency");
            entity.Property(e => e.PerceptionType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("perception_type");
            entity.Property(e => e.PerformanceBonus)
                .HasDefaultValue(0)
                .HasColumnName("performance_bonus");
            entity.Property(e => e.PerformanceProficiency)
                .HasDefaultValue(0)
                .HasColumnName("performance_proficiency");
            entity.Property(e => e.PerformanceType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("performance_type");
            entity.Property(e => e.PersuasionBonus)
                .HasDefaultValue(0)
                .HasColumnName("persuasion_bonus");
            entity.Property(e => e.PersuasionProficiency)
                .HasDefaultValue(0)
                .HasColumnName("persuasion_proficiency");
            entity.Property(e => e.PersuasionType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("persuasion_type");
            entity.Property(e => e.ReligionBonus)
                .HasDefaultValue(0)
                .HasColumnName("religion_bonus");
            entity.Property(e => e.ReligionProficiency)
                .HasDefaultValue(0)
                .HasColumnName("religion_proficiency");
            entity.Property(e => e.ReligionType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("religion_type");
            entity.Property(e => e.SleightOfHandBonus)
                .HasDefaultValue(0)
                .HasColumnName("sleight_of_hand_bonus");
            entity.Property(e => e.SleightOfHandProficiency)
                .HasDefaultValue(0)
                .HasColumnName("sleight_of_hand_proficiency");
            entity.Property(e => e.SleightOfHandType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("sleight_of_hand_type");
            entity.Property(e => e.StealthBonus)
                .HasDefaultValue(0)
                .HasColumnName("stealth_bonus");
            entity.Property(e => e.StealthProficiency)
                .HasDefaultValue(0)
                .HasColumnName("stealth_proficiency");
            entity.Property(e => e.StealthType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("stealth_type");
            entity.Property(e => e.SurvivalBonus)
                .HasDefaultValue(0)
                .HasColumnName("survival_bonus");
            entity.Property(e => e.SurvivalProficiency)
                .HasDefaultValue(0)
                .HasColumnName("survival_proficiency");
            entity.Property(e => e.SurvivalType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'0'::character varying")
                .HasColumnName("survival_type");

            entity.HasOne(d => d.Character).WithOne(p => p.Skill)
                .HasForeignKey<Skill>(d => d.CharacterId)
                .HasConstraintName("fk_skill_character");
        });

        modelBuilder.Entity<Spell>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("spell_pkey");

            entity.ToTable("spell");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AtHigherLevelsDescription).HasColumnName("at_higher_levels_description");
            entity.Property(e => e.CastingTime)
                .HasMaxLength(255)
                .HasColumnName("casting_time");
            entity.Property(e => e.CharacterId).HasColumnName("character_id");
            entity.Property(e => e.Class)
                .HasMaxLength(255)
                .HasColumnName("class");
            entity.Property(e => e.ComponentDescription).HasColumnName("component_description");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Duration)
                .HasMaxLength(255)
                .HasColumnName("duration");
            entity.Property(e => e.Innate)
                .HasMaxLength(255)
                .HasColumnName("innate");
            entity.Property(e => e.IsConcentration).HasColumnName("is_concentration");
            entity.Property(e => e.IsRitual).HasColumnName("is_ritual");
            entity.Property(e => e.Level).HasColumnName("level");
            entity.Property(e => e.MComponent).HasColumnName("m_component");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Output).HasColumnName("output");
            entity.Property(e => e.Range)
                .HasMaxLength(255)
                .HasColumnName("range");
            entity.Property(e => e.SComponent).HasColumnName("s_component");
            entity.Property(e => e.School)
                .HasMaxLength(255)
                .HasColumnName("school");
            entity.Property(e => e.SpellCastingAbility)
                .HasMaxLength(255)
                .HasColumnName("spell_casting_ability");
            entity.Property(e => e.Target)
                .HasMaxLength(255)
                .HasColumnName("target");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .HasColumnName("type");
            entity.Property(e => e.VComponent).HasColumnName("v_component");

            entity.HasOne(d => d.Character).WithMany(p => p.Spells)
                .HasForeignKey(d => d.CharacterId)
                .HasConstraintName("fk_spell_character");
        });

        modelBuilder.Entity<ToolOrCustomSkillProficiency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tool_or_custom_skill_proficiency_pkey");

            entity.ToTable("tool_or_custom_skill_proficiency");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CharacterId).HasColumnName("character_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ProficiencyLevel).HasColumnName("proficiency_level");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.ToolOrCustomSkillProficiency)
                .HasForeignKey<ToolOrCustomSkillProficiency>(d => d.Id)
                .HasConstraintName("fk_tool_or_custom_skill_proficiency_character");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
