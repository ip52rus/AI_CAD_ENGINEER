namespace AI_CAD_ENGINEER.Engineering.Models;

public enum DrawingViewRole
{
    Main,
    VerticalProjection,
    SideProjection
}

public class ViewAxisMapping
{
    public DrawingViewRole ViewRole { get; set; }

    public ModelAxis HorizontalAxis { get; set; } =
        ModelAxis.Undefined;

    public ModelAxis VerticalAxis { get; set; } =
        ModelAxis.Undefined;
}