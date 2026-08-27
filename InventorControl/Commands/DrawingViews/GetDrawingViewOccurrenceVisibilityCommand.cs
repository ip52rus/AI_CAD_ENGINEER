using System.Text.Json;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetDrawingViewOccurrenceVisibilityCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetDrawingViewOccurrenceVisibilityCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_drawing_view_occurrence_visibility";

    public string Execute(
        JsonElement root)
    {
        DrawingViewCommandSupport.DrawingViewOccurrenceResolution? resolution =
            DrawingViewCommandSupport
                .ResolveDrawingViewOccurrence(
                    _inventor,
                    root,
                    out string? resolveError);

        if (resolution == null)
        {
            return DrawingViewCommandSupport
                .CreateError(
                    resolveError ??
                    "Failed to resolve DrawingView occurrence target.");
        }

        try
        {
            bool visible =
                resolution.DrawingView.GetVisibility(
                    resolution.Occurrence);

            return DrawingViewCommandSupport
                .CreateSuccess(
                    new
                    {
                        command =
                            Name,

                        drawing =
                            DrawingViewCommandSupport
                                .ReadDocument(
                                    resolution.DrawingDocument),

                        sheet =
                            resolution.Sheet.Name,

                        drawingView =
                            resolution.DrawingView.Name,

                        target =
                            DrawingViewCommandSupport
                                .ReadVisibilityTarget(
                                    resolution),

                        visible,

                        visibilityReadbackAvailable =
                            true,

                        diagnostics =
                            new
                            {
                                sourceAssemblyDirty =
                                    resolution
                                        .ReferencedAssemblyDocument
                                        .Dirty,
                                drawingDirty =
                                    resolution
                                        .DrawingDocument
                                        .Dirty
                            }
                    });
        }
        catch (Exception exception)
        {
            return DrawingViewCommandSupport
                .CreateError(
                    $"Failed to read visibility for occurrence path \"{resolution.OccurrencePath}\" in DrawingView \"{resolution.DrawingView.Name}\".",
                    exception.Message);
        }
    }
}
