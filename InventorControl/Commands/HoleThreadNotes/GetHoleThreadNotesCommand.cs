using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetHoleThreadNotesCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetHoleThreadNotesCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_hole_thread_notes";

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

        List<object> result =
            new();

        for (int index = 1;
             index <= notes.Count;
             index++)
        {
            HoleThreadNote note =
                notes[index];

            string formattedHoleThreadNote =
                string.Empty;

            string formattedQuantityNote =
                string.Empty;

            try
            {
                formattedHoleThreadNote =
                    note.FormattedHoleThreadNote;
            }
            catch
            {
            }

            try
            {
                formattedQuantityNote =
                    note.FormattedQuantityNote;
            }
            catch
            {
            }

            result.Add(
                new
                {
                    index,

                    text =
                        HoleThreadNoteCommandSupport
                            .SafeGetText(
                                note),

                    formattedHoleThreadNote,

                    formattedQuantityNote,

                    isHoleNote =
                        note.IsHoleNote,

                    attached =
                        note.Attached,

                    arrowheadsInside =
                        note.ArrowheadsInside,

                    leaderFromCenter =
                        note.LeaderFromCenter,

                    singleDimensionLine =
                        note.SingleDimensionLine,

                    useDefaultFormat =
                        note.UseDefaultFormat,

                    usePartUnits =
                        note.UsePartUnits,

                    tapDrill =
                        note.TapDrill,

                    rightHandedThread =
                        note.RightHandedThread,

                    textOrigin =
                        HoleThreadNoteCommandSupport
                            .SafeGetTextOrigin(
                                note),

                    rangeBox =
                        HoleThreadNoteCommandSupport
                            .SafeGetRangeBox(
                                note)
                });
        }

        return HoleThreadNoteCommandSupport
            .CreateSuccess(
                new
                {
                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    noteCount =
                        result.Count,

                    notes =
                        result
                });
    }
}
