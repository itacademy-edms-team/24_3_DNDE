using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DND5E_CE.Server.Migrations
{
    /// <inheritdoc />
    public partial class ChangeOtherProfienctOrLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:character_ability_short", "STR,DEX,CON,INT,WIS,CHA,-")
                .Annotation("Npgsql:Enum:character_attribute", "strength,dexterity,constitution,intelligence,wisdom,charisma")
                .Annotation("Npgsql:Enum:other_profiency_type", "armor,language,weapon,other");

            migrationBuilder.CreateTable(
                name: "character_class",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    source = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("character_class_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "experience_to_next_level",
                columns: table => new
                {
                    level = table.Column<int>(type: "integer", nullable: false),
                    experience = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("experience_to_next_level_pkey", x => x.level);
                });

            migrationBuilder.CreateTable(
                name: "character",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    race = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    subrace = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    class_id = table.Column<int>(type: "integer", nullable: true),
                    subclass = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    creature_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    level = table.Column<int>(type: "integer", nullable: true, defaultValue: 1),
                    experience = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    inspiration = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    initiative = table.Column<int>(type: "integer", nullable: true),
                    proficiency_bonus = table.Column<int>(type: "integer", nullable: true),
                    hit_points_current = table.Column<int>(type: "integer", nullable: true),
                    hit_points_max = table.Column<int>(type: "integer", nullable: true),
                    hit_points_temp = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    death_save_success = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    death_save_fail = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    speed_base = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    spell_save_dc = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    spell_casting_ability = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    spell_attack_bonus = table.Column<int>(type: "integer", nullable: true),
                    cp = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    sp = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    ep = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    gp = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    pp = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    personality_traits = table.Column<string>(type: "text", nullable: true, defaultValueSql: "''::text"),
                    ideals = table.Column<string>(type: "text", nullable: true, defaultValueSql: "''::text"),
                    bonds = table.Column<string>(type: "text", nullable: true, defaultValueSql: "''::text"),
                    flaws = table.Column<string>(type: "text", nullable: true, defaultValueSql: "''::text")
                },
                constraints: table =>
                {
                    table.PrimaryKey("character_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_character_character_class",
                        column: x => x.class_id,
                        principalTable: "character_class",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ability",
                columns: table => new
                {
                    character_id = table.Column<int>(type: "integer", nullable: false),
                    strength = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    strength_base = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    strength_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    dexterity = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    dexterity_base = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    dexterity_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    constitution = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    constitution_base = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    constitution_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    intelligence = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    intelligence_base = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    intelligence_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    wisdom = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    wisdom_base = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    wisdom_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    charisma = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    charisma_base = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    charisma_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ability_pkey", x => x.character_id);
                    table.ForeignKey(
                        name: "fk_ability_character",
                        column: x => x.character_id,
                        principalTable: "character",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attack_and_spellcasting",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    character_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    range = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    magic_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    is_proficient = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    crit_range = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    damage1_dice = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'1d6'::character varying"),
                    Damage1Ability = table.Column<string>(type: "text", nullable: false),
                    damage1_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    damage1_crit_dice = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'1d6'::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("attack_and_spellcasting_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_attack_and_spellcasting_character",
                        column: x => x.character_id,
                        principalTable: "character",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bio",
                columns: table => new
                {
                    character_id = table.Column<int>(type: "integer", nullable: false),
                    character_appearance = table.Column<string>(type: "text", nullable: true),
                    backstory = table.Column<string>(type: "text", nullable: true),
                    allies_and_organisations = table.Column<string>(type: "text", nullable: true),
                    additional_feature_and_traits = table.Column<string>(type: "text", nullable: true),
                    treasure = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("bio_pkey", x => x.character_id);
                    table.ForeignKey(
                        name: "fk_character_bio",
                        column: x => x.character_id,
                        principalTable: "character",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "class_resource",
                columns: table => new
                {
                    character_id = table.Column<int>(type: "integer", nullable: false),
                    total = table.Column<int>(type: "integer", nullable: true),
                    value = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("class_resource_pkey", x => x.character_id);
                    table.ForeignKey(
                        name: "fk_class_resource_character",
                        column: x => x.character_id,
                        principalTable: "character",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "global_damage_modifier",
                columns: table => new
                {
                    character_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    damage_dice = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    critical_damage_dice = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("global_damage_modifier_pkey", x => x.character_id);
                    table.ForeignKey(
                        name: "fk_global_damage_modifier_character",
                        column: x => x.character_id,
                        principalTable: "character",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "other_profiency_or_language",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    character_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "text", nullable: true),
                    profiency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("other_profiency_or_language_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_other_profiency_or_language_character",
                        column: x => x.character_id,
                        principalTable: "character",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "other_resource",
                columns: table => new
                {
                    character_id = table.Column<int>(type: "integer", nullable: false),
                    total = table.Column<int>(type: "integer", nullable: true),
                    value = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("other_resource_pkey", x => x.character_id);
                    table.ForeignKey(
                        name: "fk_other_resource_character",
                        column: x => x.character_id,
                        principalTable: "character",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "skill",
                columns: table => new
                {
                    character_id = table.Column<int>(type: "integer", nullable: false),
                    acrobatics_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    acrobatics_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    acrobatics_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    animal_handling_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    animal_handling_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    animal_handling_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    arcana_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    arcana_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    arcana_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    athletics_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    athletics_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    athletics_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    deception_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    deception_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    deception_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    history_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    history_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    history_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    insight_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    insight_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    insight_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    intimidation_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    intimidation_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    intimidation_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    investigation_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    investigation_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    investigation_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    medicine_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    medicine_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    medicine_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    nature_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    nature_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    nature_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    perception_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    perception_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    perception_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    performance_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    performance_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    performance_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    persuasion_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    persuasion_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    persuasion_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    religion_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    religion_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    religion_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    sleight_of_hand_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    sleight_of_hand_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    sleight_of_hand_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    stealth_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    stealth_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    stealth_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    survival_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'0'::character varying"),
                    survival_bonus = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    survival_proficiency = table.Column<int>(type: "integer", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("skill_pkey", x => x.character_id);
                    table.ForeignKey(
                        name: "fk_skill_character",
                        column: x => x.character_id,
                        principalTable: "character",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "spell",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    character_id = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    school = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_ritual = table.Column<bool>(type: "boolean", nullable: true),
                    casting_time = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    range = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    target = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    v_component = table.Column<bool>(type: "boolean", nullable: true),
                    s_component = table.Column<bool>(type: "boolean", nullable: true),
                    m_component = table.Column<bool>(type: "boolean", nullable: true),
                    component_description = table.Column<string>(type: "text", nullable: true),
                    is_concentration = table.Column<bool>(type: "boolean", nullable: true),
                    duration = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    spell_casting_ability = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    innate = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    output = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    at_higher_levels_description = table.Column<string>(type: "text", nullable: true),
                    @class = table.Column<string>(name: "class", type: "character varying(255)", maxLength: 255, nullable: true),
                    type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("spell_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_spell_character",
                        column: x => x.character_id,
                        principalTable: "character",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tool_or_custom_skill_proficiency",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    character_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    proficiency_level = table.Column<int>(type: "integer", nullable: true),
                    Attribute = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tool_or_custom_skill_proficiency_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_tool_or_custom_skill_proficiency_character",
                        column: x => x.id,
                        principalTable: "character",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attack_and_spellcasting_character_id",
                table: "attack_and_spellcasting",
                column: "character_id");

            migrationBuilder.CreateIndex(
                name: "IX_character_class_id",
                table: "character",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_other_profiency_or_language_character_id",
                table: "other_profiency_or_language",
                column: "character_id");

            migrationBuilder.CreateIndex(
                name: "IX_spell_character_id",
                table: "spell",
                column: "character_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ability");

            migrationBuilder.DropTable(
                name: "attack_and_spellcasting");

            migrationBuilder.DropTable(
                name: "bio");

            migrationBuilder.DropTable(
                name: "class_resource");

            migrationBuilder.DropTable(
                name: "experience_to_next_level");

            migrationBuilder.DropTable(
                name: "global_damage_modifier");

            migrationBuilder.DropTable(
                name: "other_profiency_or_language");

            migrationBuilder.DropTable(
                name: "other_resource");

            migrationBuilder.DropTable(
                name: "skill");

            migrationBuilder.DropTable(
                name: "spell");

            migrationBuilder.DropTable(
                name: "tool_or_custom_skill_proficiency");

            migrationBuilder.DropTable(
                name: "character");

            migrationBuilder.DropTable(
                name: "character_class");
        }
    }
}
