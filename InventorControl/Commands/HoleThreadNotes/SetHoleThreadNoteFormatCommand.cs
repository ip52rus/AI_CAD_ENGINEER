using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SetHoleThreadNoteFormatCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SetHoleThreadNoteFormatCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "set_hole_thread_note_format";

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

        bool hasChange =
            false;

        string? formattedHoleThreadNote =
            null;

        string? formattedQuantityNote =
            null;

        bool? arrowheadsInside =
            null;

        bool? leaderFromCenter =
            null;

        bool? singleDimensionLine =
            null;

        bool? useDefaultFormat =
            null;

        bool? usePartUnits =
            null;

        bool? tapDrill =
            null;

        if (root.TryGetProperty(
                "formattedHoleThreadNote",
                out JsonElement holeFormatElement))
        {
            if (holeFormatElement.ValueKind !=
                JsonValueKind.String)
            {
                return HoleThreadNoteCommandSupport
                    .CreateError(
                        "Поле \"formattedHoleThreadNote\" " +
                        "должно быть строкой.");
            }

            formattedHoleThreadNote =
                holeFormatElement.GetString()
                ?? string.Empty;

            hasChange =
                true;
        }

        if (root.TryGetProperty(
                "formattedQuantityNote",
                out JsonElement quantityFormatElement))
        {
            if (quantityFormatElement.ValueKind !=
                JsonValueKind.String)
            {
                return HoleThreadNoteCommandSupport
                    .CreateError(
                        "Поле \"formattedQuantityNote\" " +
                        "должно быть строкой.");
            }

            formattedQuantityNote =
                quantityFormatElement.GetString()
                ?? string.Empty;

            hasChange =
                true;
        }

        if (!TryReadOptionalBoolean(
                root,
                "arrowheadsInside",
                out arrowheadsInside,
                out string arrowheadsInsideError))
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    arrowheadsInsideError);
        }

        if (!TryReadOptionalBoolean(
                root,
                "leaderFromCenter",
                out leaderFromCenter,
                out string leaderFromCenterError))
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    leaderFromCenterError);
        }

        if (!TryReadOptionalBoolean(
                root,
                "singleDimensionLine",
                out singleDimensionLine,
                out string singleDimensionLineError))
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    singleDimensionLineError);
        }

        if (!TryReadOptionalBoolean(
                root,
                "useDefaultFormat",
                out useDefaultFormat,
                out string useDefaultFormatError))
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    useDefaultFormatError);
        }

        if (!TryReadOptionalBoolean(
                root,
                "usePartUnits",
                out usePartUnits,
                out string usePartUnitsError))
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    usePartUnitsError);
        }

        if (!TryReadOptionalBoolean(
                root,
                "tapDrill",
                out tapDrill,
                out string tapDrillError))
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    tapDrillError);
        }

        hasChange =
            hasChange ||
            arrowheadsInside.HasValue ||
            leaderFromCenter.HasValue ||
            singleDimensionLine.HasValue ||
            useDefaultFormat.HasValue ||
            usePartUnits.HasValue ||
            tapDrill.HasValue;

        if (!hasChange)
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    "Не передано ни одного параметра для изменения.");
        }

        string previousFormattedHoleThreadNote =
            note.FormattedHoleThreadNote;

        string previousFormattedQuantityNote =
            note.FormattedQuantityNote;

        bool previousArrowheadsInside =
            note.ArrowheadsInside;

        bool previousLeaderFromCenter =
            note.LeaderFromCenter;

        bool previousSingleDimensionLine =
            note.SingleDimensionLine;

        bool previousUseDefaultFormat =
            note.UseDefaultFormat;

        bool previousUsePartUnits =
            note.UsePartUnits;

        bool previousTapDrill =
            note.TapDrill;

        try
        {
            if (useDefaultFormat.HasValue)
            {
                note.UseDefaultFormat =
                    useDefaultFormat.Value;
            }

            if (formattedHoleThreadNote != null)
            {
                note.FormattedHoleThreadNote =
                    formattedHoleThreadNote;
            }

            if (formattedQuantityNote != null)
            {
                note.FormattedQuantityNote =
                    formattedQuantityNote;
            }

            if (arrowheadsInside.HasValue)
            {
                note.ArrowheadsInside =
                    arrowheadsInside.Value;
            }

            if (leaderFromCenter.HasValue)
            {
                note.LeaderFromCenter =
                    leaderFromCenter.Value;
            }

            if (singleDimensionLine.HasValue)
            {
                note.SingleDimensionLine =
                    singleDimensionLine.Value;
            }

            if (usePartUnits.HasValue)
            {
                note.UsePartUnits =
                    usePartUnits.Value;
            }

            if (tapDrill.HasValue)
            {
                note.TapDrill =
                    tapDrill.Value;
            }

            drawingDocument.Update();

            return HoleThreadNoteCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            "Формат обозначения отверстия/резьбы изменён.",

                        document =
                            drawingDocument.DisplayName,

                        sheet =
                            sheet.Name,

                        noteIndex,

                        previous =
                            new
                            {
                                formattedHoleThreadNote =
                                    previousFormattedHoleThreadNote,

                                formattedQuantityNote =
                                    previousFormattedQuantityNote,

                                arrowheadsInside =
                                    previousArrowheadsInside,

                                leaderFromCenter =
                                    previousLeaderFromCenter,

                                singleDimensionLine =
                                    previousSingleDimensionLine,

                                useDefaultFormat =
                                    previousUseDefaultFormat,

                                usePartUnits =
                                    previousUsePartUnits,

                                tapDrill =
                                    previousTapDrill
                            },

                        current =
                            new
                            {
                                text =
                                    HoleThreadNoteCommandSupport
                                        .SafeGetText(
                                            note),

                                formattedHoleThreadNote =
                                    note.FormattedHoleThreadNote,

                                formattedQuantityNote =
                                    note.FormattedQuantityNote,

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

                                textOrigin =
                                    HoleThreadNoteCommandSupport
                                        .SafeGetTextOrigin(
                                            note),

                                rangeBox =
                                    HoleThreadNoteCommandSupport
                                        .SafeGetRangeBox(
                                            note)
                            },

                        dirty =
                            drawingDocument.Dirty
                    });
        }
        catch (Exception exception)
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    "Не удалось изменить формат обозначения " +
                    "отверстия/резьбы.",
                    exception.Message);
        }
    }

    private static bool TryReadOptionalBoolean(
        JsonElement root,
        string propertyName,
        out bool? value,
        out string error)
    {
        value =
            null;

        error =
            string.Empty;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return true;
        }

        if (element.ValueKind !=
                JsonValueKind.True &&
            element.ValueKind !=
                JsonValueKind.False)
        {
            error =
                $"Поле \"{propertyName}\" должно " +
                "содержать true или false.";

            return false;
        }

        value =
            element.GetBoolean();

        return true;
    }
}
