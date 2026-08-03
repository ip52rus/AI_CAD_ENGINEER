namespace AI_CAD_ENGINEER.Engineering.Decision;

public class DrawingPlan
{
    public StandardViewOrientation MainView { get; set; }

    public List<StandardViewOrientation> AdditionalViews { get; } = new();

    public double Scale { get; set; } = 1.0;

    public bool NeedSection { get; set; }

    public bool NeedCenterlines { get; set; }

    public bool NeedDimensions { get; set; }
}