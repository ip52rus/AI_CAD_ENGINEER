using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetThreadFeaturesCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetThreadFeaturesCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_thread_features";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawing =
            ModelFeatureReadSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawing == null)
        {
            return ModelFeatureReadSupport
                .CreateError(
                    documentError ??
                    "Unable to get active drawing document.");
        }

        if (!ModelFeatureReadSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetError))
        {
            return ModelFeatureReadSupport
                .CreateError(
                    sheetError);
        }

        if (!ModelFeatureReadSupport
                .TryGetRequiredString(
                    root,
                    "viewName",
                    out string viewName,
                    out string viewError))
        {
            return ModelFeatureReadSupport
                .CreateError(
                    viewError);
        }

        bool includeSuppressed =
            ModelFeatureReadSupport
                .GetOptionalBoolean(
                    root,
                    "includeSuppressed",
                    true);

        Sheet? sheet =
            ModelFeatureReadSupport
                .FindSheet(
                    drawing,
                    sheetName);

        if (sheet == null)
        {
            return ModelFeatureReadSupport
                .CreateError(
                    $"Sheet \"{sheetName}\" was not found.");
        }

        DrawingView? view =
            ModelFeatureReadSupport
                .FindView(
                    sheet,
                    viewName);

        if (view == null)
        {
            return ModelFeatureReadSupport
                .CreateError(
                    $"View \"{viewName}\" was not found.");
        }

        Document? modelDocument =
            ModelFeatureReadSupport
                .GetReferencedDocument(
                    view,
                    out string? referenceError);

        if (modelDocument == null)
        {
            return ModelFeatureReadSupport
                .CreateError(
                    "Unable to get referenced model document.",
                    referenceError);
        }

        List<object> diagnostics =
            new();

        List<object> threadFeatures =
            ModelFeatureReadSupport
                .ReadThreadFeatures(
                    modelDocument,
                    includeSuppressed,
                    diagnostics);

        return ModelFeatureReadSupport
            .CreateSuccess(
                new
                {
                    document =
                        drawing.DisplayName,

                    sheet =
                        sheet.Name,

                    view =
                        view.Name,

                    modelDocument =
                        ModelFeatureReadSupport
                            .ReadDocument(
                                modelDocument),

                    capability =
                        "thread_features",

                    includeSuppressed,

                    count =
                        threadFeatures.Count,

                    items =
                        threadFeatures,

                    diagnostics
                });
    }
}
