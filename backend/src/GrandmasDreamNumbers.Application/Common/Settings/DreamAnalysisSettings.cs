namespace GrandmasDreamNumbers.Application.Common.Settings;

public class DreamAnalysisSettings
{
    public const string SectionName = "DreamAnalysis";

    public int MaxDreamTextLength { get; set; } = 4000;
    public int CombinationSize { get; set; } = 6;
    public int MaxSuggestedCombinations { get; set; } = 5;
}
