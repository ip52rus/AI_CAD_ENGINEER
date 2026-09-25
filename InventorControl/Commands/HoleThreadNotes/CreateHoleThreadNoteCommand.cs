using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class CreateHoleThreadNoteCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public CreateHoleThreadNoteCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_hole_thread_note";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            HoleThreadNoteCommandSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawingDocument == null)
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        if (!HoleThreadNoteCommandSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetNameError))
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    sheetNameError);
        }

        if (!HoleThreadNoteCommandSupport
                .TryGetRequiredString(
                    root,
                    "viewName",
                    out string viewName,
                    out string viewNameError))
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    viewNameError);
        }

        if (!HoleThreadNoteCommandSupport
                .TryGetRequiredInt32(
                    root,
                    "curveIndex",
                    out int curveIndex,
                    out string curveIndexError))
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    curveIndexError);
        }

        if (!HoleThreadNoteCommandSupport
                .TryGetRequiredDouble(
                    root,
                    "x",
                    out double x,
                    out string xError))
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    xError);
        }

        if (!HoleThreadNoteCommandSupport
                .TryGetRequiredDouble(
                    root,
                    "y",
                    out double y,
                    out string yError))
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    yError);
        }

        if (!HoleThreadNoteCommandSupport
                .TryGetOptionalBoolean(
                    root,
                    "linearDiameterType",
                    false,
                    out bool linearDiameterType,
                    out string linearDiameterTypeError))
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    linearDiameterTypeError);
        }

        string dimensionStyle =
            string.Empty;

        if (root.TryGetProperty(
                "dimensionStyle",
                out JsonElement dimensionStyleElement))
        {
            if (dimensionStyleElement.ValueKind !=
                JsonValueKind.String)
            {
                return HoleThreadNoteCommandSupport
                    .CreateError(
                        "Поле \"dimensionStyle\" " +
                        "должно быть строкой.");
            }

            dimensionStyle =
                dimensionStyleElement
                    .GetString()?
                    .Trim()
                ?? string.Empty;
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport
                .FindSheet(
                    drawingDocument,
                    sheetName);

        if (sheet == null)
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    $"Лист \"{sheetName}\" не найден.");
        }

        DrawingView? view =
            HoleThreadNoteCommandSupport
                .FindView(
                    sheet,
                    viewName);

        if (view == null)
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    $"Вид \"{viewName}\" не найден " +
                    $"на листе \"{sheet.Name}\".");
        }

        List<DrawingCurve> curves;

        try
        {
            curves =
                HoleThreadNoteCommandSupport
                    .GetCurves(
                        view);
        }
        catch (Exception exception)
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    $"Не удалось получить кривые вида " +
                    $"\"{view.Name}\".",
                    exception.Message);
        }

        if (curveIndex < 1 ||
            curveIndex > curves.Count)
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    $"curveIndex должен быть от 1 до " +
                    $"{curves.Count}.");
        }

        DrawingCurve curve =
            curves[curveIndex - 1];

        Point2d position =
            _inventor
                .TransientGeometry
                .CreatePoint2d(
                    x,
                    y);

        HoleThreadNotes notes =
            sheet
                .DrawingNotes
                .HoleThreadNotes;

        try
        {
            HoleThreadNote note;

            if (string.IsNullOrWhiteSpace(
                    dimensionStyle))
            {
                note =
                    notes.Add(
                        position,
                        curve,
                        linearDiameterType,
                        Type.Missing);
            }
            else
            {
                note =
                    notes.Add(
                        position,
                        curve,
                        linearDiameterType,
                        dimensionStyle);
            }

            drawingDocument.Update();

            int createdIndex =
                notes.Count;

            return HoleThreadNoteCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            "Обозначение отверстия/резьбы создано.",

                        document =
                            drawingDocument.DisplayName,

                        sheet =
                            sheet.Name,

                        view =
                            view.Name,

                        index =
                            createdIndex,

                        curveIndex,

                        curveType =
                            curve.CurveType.ToString(),

                        position =
                            new
                            {
                                x,
                                y
                            },

                        linearDiameterType,

                        dimensionStyle =
                            string.IsNullOrWhiteSpace(
                                dimensionStyle)
                                ? null
                                : dimensionStyle,

                        text =
                            HoleThreadNoteCommandSupport
                                .SafeGetText(
                                    note),

                        formattedHoleThreadNote =
                            note.FormattedHoleThreadNote,

                        formattedQuantityNote =
                            note.FormattedQuantityNote,

                        isHoleNote =
                            note.IsHoleNote,

                        attached =
                            note.Attached,

                        textOrigin =
                            HoleThreadNoteCommandSupport
                                .SafeGetTextOrigin(
                                    note),

                        rangeBox =
                            HoleThreadNoteCommandSupport
                                .SafeGetRangeBox(
                                    note),

                        dirty =
                            drawingDocument.Dirty
                    });
        }
        catch (Exception exception)
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    "Не удалось создать обозначение " +
                    "отверстия/резьбы.",
                    "Выбранная кривая должна представлять " +
                    "ребро отверстия или резьбы. " +
                    exception.Message);
        }
    }
}
