namespace AI_CAD_ENGINEER.Engineering.Models;

public class HoleAnalysisResult
{
    public bool IsSheetMetal { get; set; }

    public double? SheetMetalThickness { get; set; }

    public int FeatureCount { get; set; }

    public List<HoleInfo> Holes { get; } = new();

    public int PhysicalHoleCount =>
        Holes.Count;

    public List<HoleFeatureAnalysis> Features { get; } = new();
}

public class HoleFeatureAnalysis
{
    public string Name { get; set; } = string.Empty;

    public double Diameter { get; set; }

    public double Depth { get; set; }

    public bool HasThroughAllExtent { get; set; }

    public bool PassesThroughSheetMetal { get; set; }

    public bool IsThroughHole { get; set; }

    public int PhysicalHoleCount { get; set; }
}