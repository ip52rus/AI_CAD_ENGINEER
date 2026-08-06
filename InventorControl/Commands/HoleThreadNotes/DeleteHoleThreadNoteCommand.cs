using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class DeleteHoleThreadNoteCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public DeleteHoleThreadNoteCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_hole_thread_note";

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

        string previousText =
            HoleThreadNoteCommandSupport
                .SafeGetText(
                    note);

        string previousFormattedHoleThreadNote =
            string.Empty;

        string previousFormattedQuantityNote =
            string.Empty;

        try
        {
            previousFormattedHoleThreadNote =
                note.FormattedHoleThreadNote;
        }
        catch
        {
        }

        try
        {
            previousFormattedQuantityNote =
                note.FormattedQuantityNote;
        }
        catch
        {
        }

        try
        {
            note.Delete();

            drawingDocument.Update();

            return HoleThreadNoteCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            "Обозначение отверстия/резьбы удалено.",

                        document =
                            drawingDocument.DisplayName,

                        sheet =
                            sheet.Name,

                        deletedIndex =
                            noteIndex,

                        previousText,

                        previousFormattedHoleThreadNote,

                        previousFormattedQuantityNote,

                        remainingNoteCount =
                            notes.Count,

                        dirty =
                            drawingDocument.Dirty
                    });
        }
        catch (Exception exception)
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    "Не удалось удалить обозначение " +
                    "отверстия/резьбы.",
                    exception.Message);
        }
    }
}
