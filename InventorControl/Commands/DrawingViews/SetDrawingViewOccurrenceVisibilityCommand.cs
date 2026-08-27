using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SetDrawingViewOccurrenceVisibilityCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SetDrawingViewOccurrenceVisibilityCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "set_drawing_view_occurrence_visibility";

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

        if (!DrawingViewCommandSupport
                .TryGetRequiredBoolean(
                    root,
                    "visible",
                    out bool visible,
                    out string visibleError))
        {
            return DrawingViewCommandSupport
                .CreateError(
                    visibleError);
        }

        bool? previousVisible =
            null;

        string? previousReadbackError =
            null;

        try
        {
            previousVisible =
                resolution.DrawingView.GetVisibility(
                    resolution.Occurrence);
        }
        catch (Exception exception)
        {
            previousReadbackError =
                exception.Message;
        }

        try
        {
            resolution.DrawingView.SetVisibility(
                resolution.Occurrence,
                visible);

            resolution.DrawingDocument.Update();

            bool? actualVisible =
                null;

            string? actualReadbackError =
                null;

            try
            {
                actualVisible =
                    resolution.DrawingView.GetVisibility(
                        resolution.Occurrence);
            }
            catch (Exception exception)
            {
                actualReadbackError =
                    exception.Message;
            }

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

                        requestedVisible =
                            visible,

                        previousVisible,

                        actualVisible,

                        visibilityReadbackAvailable =
                            actualVisible.HasValue,

                        diagnostics =
                            new
                            {
                                previousReadbackError,
                                actualReadbackError,
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
                    $"Failed to set visibility for occurrence path \"{resolution.OccurrencePath}\" in DrawingView \"{resolution.DrawingView.Name}\".",
                    exception.Message);
        }
    }
}
