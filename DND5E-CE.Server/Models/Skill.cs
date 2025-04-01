using System;
using System.Collections.Generic;

namespace DND5E_CE.Server.Models;

public partial class Skill
{
    public int CharacterId { get; set; }

    public string? AcrobaticsType { get; set; }

    public int? AcrobaticsBonus { get; set; }

    public int? AcrobaticsProficiency { get; set; }

    public string? AnimalHandlingType { get; set; }

    public int? AnimalHandlingBonus { get; set; }

    public int? AnimalHandlingProficiency { get; set; }

    public string? ArcanaType { get; set; }

    public int? ArcanaBonus { get; set; }

    public int? ArcanaProficiency { get; set; }

    public string? AthleticsType { get; set; }

    public int? AthleticsBonus { get; set; }

    public int? AthleticsProficiency { get; set; }

    public string? DeceptionType { get; set; }

    public int? DeceptionBonus { get; set; }

    public int? DeceptionProficiency { get; set; }

    public string? HistoryType { get; set; }

    public int? HistoryBonus { get; set; }

    public int? HistoryProficiency { get; set; }

    public string? InsightType { get; set; }

    public int? InsightBonus { get; set; }

    public int? InsightProficiency { get; set; }

    public string? IntimidationType { get; set; }

    public int? IntimidationBonus { get; set; }

    public int? IntimidationProficiency { get; set; }

    public string? InvestigationType { get; set; }

    public int? InvestigationBonus { get; set; }

    public int? InvestigationProficiency { get; set; }

    public string? MedicineType { get; set; }

    public int? MedicineBonus { get; set; }

    public int? MedicineProficiency { get; set; }

    public string? NatureType { get; set; }

    public int? NatureBonus { get; set; }

    public int? NatureProficiency { get; set; }

    public string? PerceptionType { get; set; }

    public int? PerceptionBonus { get; set; }

    public int? PerceptionProficiency { get; set; }

    public string? PerformanceType { get; set; }

    public int? PerformanceBonus { get; set; }

    public int? PerformanceProficiency { get; set; }

    public string? PersuasionType { get; set; }

    public int? PersuasionBonus { get; set; }

    public int? PersuasionProficiency { get; set; }

    public string? ReligionType { get; set; }

    public int? ReligionBonus { get; set; }

    public int? ReligionProficiency { get; set; }

    public string? SleightOfHandType { get; set; }

    public int? SleightOfHandBonus { get; set; }

    public int? SleightOfHandProficiency { get; set; }

    public string? StealthType { get; set; }

    public int? StealthBonus { get; set; }

    public int? StealthProficiency { get; set; }

    public string? SurvivalType { get; set; }

    public int? SurvivalBonus { get; set; }

    public int? SurvivalProficiency { get; set; }

    public virtual Character Character { get; set; } = null!;
}
