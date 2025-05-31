using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.DTO.App.Sheet1
{
    public class SkillDto
    {
        [Required]
        public bool IsAcrobaticsProficient { get; set; }
        [Required]
        public string AcrobaticsBondAbility { get; set; }

        [Required]
        public bool IsAnimalHandlingProficient { get; set; }
        [Required]
        public string AnimalHandlingBondAbility { get; set; }

        [Required]
        public bool IsArcanaProficient { get; set; }
        [Required]
        public string ArcanaBondAbility { get; set; }

        [Required]
        public bool IsAthleticsProficient { get; set; }
        [Required]
        public string AthleticsBondAbility { get; set; }

        [Required]
        public bool IsDeceptionProficient { get; set; }
        [Required]
        public string DeceptionBondAbility { get; set; }

        [Required]
        public bool IsHistoryProficient { get; set; }
        [Required]
        public string HistoryBondAbility { get; set; }

        [Required]
        public bool IsInsightProficient { get; set; }
        [Required]
        public string InsightBondAbility { get; set; }

        [Required]
        public bool IsIntimidationProficient { get; set; }
        [Required]
        public string IntimidationBondAbility { get; set; }

        [Required]
        public bool IsInvestigationProficient { get; set; }
        [Required]
        public string InvestigationBondAbility { get; set; }

        [Required]
        public bool IsMedicineProficient { get; set; }
        [Required]
        public string MedicineBondAbility { get; set; }

        [Required]
        public bool IsNatureProficient { get; set; }
        [Required]
        public string NatureBondAbility { get; set; }

        [Required]
        public bool IsPerceptionProficient { get; set; }
        [Required]
        public string PerceptionBondAbility { get; set; }

        [Required]
        public bool IsPerformanceProficient { get; set; }
        [Required]
        public string PerformanceBondAbility { get; set; }

        [Required]
        public bool IsPersuasionProficient { get; set; }
        [Required]
        public string PersuasionBondAbility { get; set; }

        [Required]
        public bool IsReligionProficient { get; set; }
        [Required]
        public string ReligionBondAbility { get; set; }

        [Required]
        public bool IsSleightOfHandProficient { get; set; }
        [Required]
        public string SleightOfHandBondAbility { get; set; }

        [Required]
        public bool IsStealthProficient { get; set; }
        [Required]
        public string StealthBondAbility { get; set; }

        [Required]
        public bool IsSurvivalProficient { get; set; }
        [Required]
        public string SurvivalBondAbility { get; set; }
    }
}
