using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateRevisionCloudCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateRevisionCloudCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_revision_cloud";

    public string Execute(
        JsonElement root)
    {
        List<object> diagnostics =
            new();

        DrawingDocument? drawingDocument =
            HoleThreadNoteCommandSupport.GetActiveDrawingDocument(
                _inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            diagnostics.Add(new { scope = "activeDocument", message = documentError ?? "Unable to resolve active DrawingDocument." });
            return RevisionCloudCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!RevisionCloudCommandSupport.TryGetCreateInputs(
                root,
                diagnostics,
                out string sheetName,
                out List<(double X, double Y)> controlPointInputs,
                out bool inverted,
                out string? name,
                out bool nameSupplied))
        {
            return RevisionCloudCommandSupport.CreateError(
                "Invalid create_revision_cloud input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return RevisionCloudCommandSupport.CreateError(
                "Invalid create_revision_cloud input.",
                diagnostics);
        }

        RevisionClouds clouds;

        try
        {
            clouds =
                sheet.RevisionClouds;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.RevisionClouds", message = exception.Message, exceptionType = exception.GetType().FullName });
            return RevisionCloudCommandSupport.CreateError(
                "Unable to access RevisionClouds collection.",
                diagnostics);
        }

        int? countBefore =
            RevisionCloudCommandSupport.ReadRevisionCloudCount(
                sheet,
                diagnostics,
                "Sheet.RevisionClouds.Count.BeforeCreate");

        RevisionCloudDefinition definition;

        try
        {
            ObjectCollection controlPoints =
                _inventor
                    .TransientObjects
                    .CreateObjectCollection();

            foreach ((double x, double y) in controlPointInputs)
            {
                controlPoints.Add(
                    _inventor
                        .TransientGeometry
                        .CreatePoint2d(
                            x,
                            y));
            }

            definition =
                clouds.CreateRevisionCloudDefinition(
                    controlPoints,
                    Type.Missing,
                    inverted);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "RevisionClouds.CreateRevisionCloudDefinition", message = exception.Message, exceptionType = exception.GetType().FullName, controlPointCount = controlPointInputs.Count, inverted });
            return RevisionCloudCommandSupport.CreateError(
                "Failed to create revision cloud definition.",
                diagnostics);
        }

        RevisionCloud createdRevisionCloud;

        try
        {
            createdRevisionCloud =
                nameSupplied
                    ? clouds.Add(
                        definition,
                        name!)
                    : clouds.Add(
                        definition,
                        Type.Missing);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "RevisionClouds.Add", message = exception.Message, exceptionType = exception.GetType().FullName, nameSupplied, name });
            return RevisionCloudCommandSupport.CreateError(
                "Failed to create revision cloud.",
                diagnostics);
        }

        if (createdRevisionCloud == null)
        {
            diagnostics.Add(new { scope = "RevisionClouds.Add", message = "Inventor returned null RevisionCloud." });
            return RevisionCloudCommandSupport.CreateError(
                "Failed to create revision cloud.",
                diagnostics);
        }

        int? countAfter =
            RevisionCloudCommandSupport.ReadRevisionCloudCount(
                sheet,
                diagnostics,
                "Sheet.RevisionClouds.Count.AfterCreate");

        int createdRevisionCloudIndex =
            countAfter ??
            countBefore.GetValueOrDefault() + 1;

        object revisionCloud =
            RevisionCloudReadSupport.ReadRevisionCloudSnapshot(
                drawingDocument,
                sheet,
                createdRevisionCloud,
                createdRevisionCloudIndex);

        bool verificationPassed =
            true;

        if (countBefore.HasValue &&
            countAfter.HasValue &&
            countAfter.Value != countBefore.Value + 1)
        {
            diagnostics.Add(new { scope = "RevisionCloud.Create.Verification.Count", message = "Sheet.RevisionClouds.Count did not increase by exactly 1.", countBefore, countAfter });
            verificationPassed =
                false;
        }

        bool? actualInverted =
            RevisionCloudCommandSupport.ReadInverted(
                createdRevisionCloud,
                diagnostics,
                "RevisionCloud.Create.Verification.Inverted");

        if (actualInverted.HasValue &&
            actualInverted.Value != inverted)
        {
            diagnostics.Add(new { scope = "RevisionCloud.Create.Verification.Inverted", message = "Inventor did not report the requested inverted state after creation.", requested = inverted, actual = actualInverted.Value });
            verificationPassed =
                false;
        }

        List<(double X, double Y)> actualControlPoints =
            RevisionCloudCommandSupport.ReadControlPointPositions(
                createdRevisionCloud,
                diagnostics,
                "RevisionCloud.Create.Verification.ControlPoints");

        if (actualControlPoints.Count !=
            controlPointInputs.Count)
        {
            diagnostics.Add(new { scope = "RevisionCloud.Create.Verification.ControlPointCount", message = "Inventor reported a factual control-point count different from the requested count.", requestedCount = controlPointInputs.Count, actualCount = actualControlPoints.Count });
        }

        if (!RevisionCloudCommandSupport.ControlPointPositionsMatchPrefix(
                actualControlPoints,
                controlPointInputs))
        {
            diagnostics.Add(new { scope = "RevisionCloud.Create.Verification.ControlPointPositions", message = "Inventor did not preserve the caller-supplied control-point positions as the factual leading control points.", requestedControlPoints = controlPointInputs.Select(point => new { x = point.X, y = point.Y }).ToList(), actualControlPoints = actualControlPoints.Select(point => new { x = point.X, y = point.Y }).ToList() });
            verificationPassed =
                false;
        }

        if (!verificationPassed)
        {
            return RevisionCloudCommandSupport.CreateError(
                "Revision cloud creation was not factually verified.",
                diagnostics);
        }

        return RevisionCloudCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "create_revision_cloud",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                requestedControlPoints =
                    controlPointInputs.Select(
                            point => new
                            {
                                x =
                                    point.X,
                                y =
                                    point.Y
                            })
                        .ToList(),
                inverted,
                requestedName =
                    nameSupplied
                        ? name
                        : null,
                createdRevisionCloudIndex,
                countBefore,
                countAfter,
                revisionCloud,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
