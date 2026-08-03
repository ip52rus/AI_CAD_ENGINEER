namespace AI_CAD_ENGINEER.Core;

public class ViewStatistics
{
    public double Width { get; set; }

    public double Height { get; set; }

    public double Area { get; set; }

    public double AspectRatio { get; set; }

    public int CurveCount { get; set; }

    public int SegmentCount { get; set; }

    public int LineCount { get; set; }

    public int CircleCount { get; set; }

    public int ArcCount { get; set; }

    public int EllipticalArcCount { get; set; }

    public int OtherGeometryCount { get; set; }
}