using AI_CAD_ENGINEER.Engineering.Models;

namespace AI_CAD_ENGINEER.Engineering.Decision;

public class ViewAxisMappingResolver
{
    public Dictionary<DrawingViewRole, ViewAxisMapping> Resolve(
        StandardViewOrientation mainOrientation)
    {
        ModelAxis horizontalAxis;
        ModelAxis verticalAxis;
        ModelAxis hiddenAxis;

        switch (mainOrientation)
        {
            case StandardViewOrientation.Front:
            case StandardViewOrientation.Back:

                horizontalAxis =
                    ModelAxis.X;

                verticalAxis =
                    ModelAxis.Y;

                hiddenAxis =
                    ModelAxis.Z;

                break;

            case StandardViewOrientation.Top:
            case StandardViewOrientation.Bottom:

                horizontalAxis =
                    ModelAxis.X;

                verticalAxis =
                    ModelAxis.Z;

                hiddenAxis =
                    ModelAxis.Y;

                break;

            case StandardViewOrientation.Left:
            case StandardViewOrientation.Right:

                horizontalAxis =
                    ModelAxis.Z;

                verticalAxis =
                    ModelAxis.Y;

                hiddenAxis =
                    ModelAxis.X;

                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(mainOrientation),
                    mainOrientation,
                    "Неизвестная ориентация главного вида.");
        }

        return new Dictionary<DrawingViewRole, ViewAxisMapping>
        {
            [DrawingViewRole.Main] =
                new ViewAxisMapping
                {
                    ViewRole =
                        DrawingViewRole.Main,

                    HorizontalAxis =
                        horizontalAxis,

                    VerticalAxis =
                        verticalAxis
                },

            [DrawingViewRole.VerticalProjection] =
                new ViewAxisMapping
                {
                    ViewRole =
                        DrawingViewRole.VerticalProjection,

                    HorizontalAxis =
                        horizontalAxis,

                    VerticalAxis =
                        hiddenAxis
                },

            [DrawingViewRole.SideProjection] =
                new ViewAxisMapping
                {
                    ViewRole =
                        DrawingViewRole.SideProjection,

                    HorizontalAxis =
                        hiddenAxis,

                    VerticalAxis =
                        verticalAxis
                }
        };
    }
}