using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class MoveHoleThreadNoteCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public MoveHoleThreadNoteCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_hole_thread_note";

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
                .TryGetRequiredInt32(
                    root,
                    "noteIndex",
                    out int noteIndex,
                    out string noteIndexError))
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    noteIndexError);
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

        HoleThreadNotes notes =
            sheet
                .DrawingNotes
                .HoleThreadNotes;

        if (noteIndex < 1 ||
            noteIndex > notes.Count)
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    $"noteIndex должен быть от 1 до " +
                    $"{notes.Count}.");
        }

        HoleThreadNote note =
            notes[noteIndex];

        object? previousTextOrigin =
            HoleThreadNoteCommandSupport
                .SafeGetTextOrigin(
                    note);

        object? previousRangeBox =
            HoleThreadNoteCommandSupport
                .SafeGetRangeBox(
                    note);

        try
        {
            Point2d newOrigin =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            dynamic textDynamic =
                note.Text;

            textDynamic.Origin =
                newOrigin;

            drawingDocument.Update();

            return HoleThreadNoteCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            "Обозначение отверстия/резьбы перемещено.",

                        document =
                            drawingDocument.DisplayName,

                        sheet =
                            sheet.Name,

                        noteIndex,

                        text =
                            HoleThreadNoteCommandSupport
                                .SafeGetText(
                                    note),

                        previousTextOrigin,

                        textOrigin =
                            HoleThreadNoteCommandSupport
                                .SafeGetTextOrigin(
                                    note),

                        previousRangeBox,

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
                    "Не удалось переместить обозначение " +
                    "отверстия/резьбы.",
                    exception.Message);
        }
    }
}
