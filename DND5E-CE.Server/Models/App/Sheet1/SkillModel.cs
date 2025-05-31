using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DND5E_CE.Server.Models.App.Sheet1
{
    public class SkillModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid Sheet1Id { get; set; }

        [ForeignKey("Sheet1Id")]
        public Sheet1Model Sheet1 { get; set; }

        [Required]
        public bool IsAcrobaticsProficient { get; set; } = false;
        [Required]
        public string AcrobaticsBondAbility { get; set; } = "Dexterity";

        [Required]
        public bool IsAnimalHandlingProficient { get; set; } = false;
        [Required]
        public string AnimalHandlingBondAbility { get; set; } = "Wisdom";

        [Required]
        public bool IsArcanaProficient { get; set; } = false;
        [Required]
        public string ArcanaBondAbility { get; set; } = "Intelligence";

        [Required]
        public bool IsAthleticsProficient { get; set; } = false;
        [Required]
        public string AthleticsBondAbility { get; set; } = "Strength";

        [Required]
        public bool IsDeceptionProficient { get; set; } = false;
        [Required]
        public string DeceptionBondAbility { get; set; } = "Charisma";

        [Required]
        public bool IsHistoryProficient { get; set; } = false;
        [Required]
        public string HistoryBondAbility { get; set; } = "Intelligence";

        [Required]
        public bool IsInsightProficient { get; set; } = false;
        [Required]
        public string InsightBondAbility { get; set; } = "Wisdom";

        [Required]
        public bool IsIntimidationProficient { get; set; } = false;
        [Required]
        public string IntimidationBondAbility { get; set; } = "Charisma";

        [Required]
        public bool IsInvestigationProficient { get; set; } = false;
        [Required]
        public string InvestigationBondAbility { get; set; } = "Intelligence";

        [Required]
        public bool IsMedicineProficient { get; set; } = false;
        [Required]
        public string MedicineBondAbility { get; set; } = "Wisdom";

        [Required]
        public bool IsNatureProficient { get; set; } = false;
        [Required]
        public string NatureBondAbility { get; set; } = "Intelligence";

        [Required]
        public bool IsPerceptionProficient { get; set; } = false;
        [Required]
        public string PerceptionBondAbility { get; set; } = "Wisdom";

        [Required]
        public bool IsPerformanceProficient { get; set; } = false;
        [Required]
        public string PerformanceBondAbility { get; set; } = "Charisma";

        [Required]
        public bool IsPersuasionProficient { get; set; } = false;
        [Required]
        public string PersuasionBondAbility { get; set; } = "Charisma";

        [Required]
        public bool IsReligionProficient { get; set; } = false;
        [Required]
        public string ReligionBondAbility { get; set; } = "Intelligence";

        [Required]
        public bool IsSleightOfHandProficient { get; set; } = false;
        [Required]
        public string SleightOfHandBondAbility { get; set; } = "Dexterity";

        [Required]
        public bool IsStealthProficient { get; set; } = false;
        [Required]
        public string StealthBondAbility { get; set; } = "Dexterity";

        [Required]
        public bool IsSurvivalProficient { get; set; } = false;
        [Required]
        public string SurvivalBondAbility { get; set; } = "Wisdom";
    }
}
