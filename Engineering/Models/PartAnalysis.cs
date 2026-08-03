namespace AI_CAD_ENGINEER.Engineering.Models;

public class PartAnalysis
{
    public string Name { get; set; } = string.Empty;

    // Размеры модели вдоль координатных осей Inventor.
    public double SizeX { get; set; }

    public double SizeY { get; set; }

    public double SizeZ { get; set; }

    // Инженерные габариты:
    // Length — наибольший размер,
    // Width — средний размер,
    // Height — наименьший размер.
    public double Length { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public double Volume { get; set; }

    public double SurfaceArea { get; set; }

    public double CenterOfMassX { get; set; }

    public double CenterOfMassY { get; set; }

    public double CenterOfMassZ { get; set; }

    // Единственный источник информации об отверстиях.
    public HoleAnalysisResult HoleAnalysis { get; set; } = new();

    public List<ChamferInfo> Chamfers { get; } = new();

    public List<FilletInfo> Fillets { get; } = new();

    public int HoleCount =>
        HoleAnalysis.PhysicalHoleCount;

    public int ChamferCount =>
        Chamfers.Count;

    public int FilletCount =>
        Fillets.Count;
}

public class HoleInfo
{
    public string Name { get; set; } = string.Empty;

    public double Diameter { get; set; }

    public double Depth { get; set; }

    public bool IsThroughHole { get; set; }

    public bool IsThreaded { get; set; }

    public string ThreadDesignation { get; set; } = string.Empty;

    public double CenterX { get; set; }

    public double CenterY { get; set; }

    public double CenterZ { get; set; }
}

public class ChamferInfo
{
    public string Name { get; set; } = string.Empty;

    public double Distance { get; set; }

    public double AngleDegrees { get; set; }
}

public class FilletInfo
{
    public string Name { get; set; } = string.Empty;

    public double Radius { get; set; }
}