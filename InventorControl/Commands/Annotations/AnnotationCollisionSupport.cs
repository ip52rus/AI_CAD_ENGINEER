using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class AnnotationCollisionSupport
{
    public static DrawingDocument? GetActiveDrawingDocument(
        Inventor.Application inventor,
        out string? error)
    {
        ArgumentNullException.ThrowIfNull(inventor);

        error = null;

        Document? activeDocument =
            inventor.ActiveDocument;

        if (activeDocument == null)
        {
            error =
                "В Inventor нет активного документа.";

            return null;
        }

        if (activeDocument.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            error =
                "Активный документ не является чертежом.";

            return null;
        }

        return
            (DrawingDocument)activeDocument;
    }

    public static Sheet? FindSheet(
        DrawingDocument drawingDocument,
        string sheetName)
    {
        foreach (Sheet sheet
                 in drawingDocument.Sheets)
        {
            if (string.Equals(
                    sheet.Name,
                    sheetName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return sheet;
            }
        }

        return null;
    }

    public static bool TryGetRequiredString(
        JsonElement root,
        string propertyName,
        out string value,
        out string error)
    {
        value =
            string.Empty;

        error =
            string.Empty;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            error =
                $"Не найдено обязательное поле \"{propertyName}\".";

            return false;
        }

        if (element.ValueKind !=
            JsonValueKind.String)
        {
            error =
                $"Поле \"{propertyName}\" должно быть строкой.";

            return false;
        }

        value =
            element.GetString()?
                .Trim()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(
                value))
        {
            error =
                $"Поле \"{propertyName}\" не должно быть пустым.";

            return false;
        }

        return true;
    }

    public static double GetOptionalDouble(
        JsonElement root,
        string propertyName,
        double defaultValue)
    {
        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return defaultValue;
        }

        return element.TryGetDouble(
            out double value)
                ? value
                : defaultValue;
    }

    public static int GetOptionalInt32(
        JsonElement root,
        string propertyName,
        int defaultValue)
    {
        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return defaultValue;
        }

        return element.TryGetInt32(
            out int value)
                ? value
                : defaultValue;
    }

    public static bool GetOptionalBoolean(
        JsonElement root,
        string propertyName,
        bool defaultValue)
    {
        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return defaultValue;
        }

        return element.ValueKind switch
        {
            JsonValueKind.True =>
                true,

            JsonValueKind.False =>
                false,

            _ =>
                defaultValue
        };
    }

    public static AnnotationBox? TryReadBox(
        string id,
        string kind,
        string text,
        Func<Box2d> getter)
    {
        try
        {
            Box2d box =
                getter();

            return new AnnotationBox(
                id,
                kind,
                text,
                box.MinPoint.X,
                box.MinPoint.Y,
                box.MaxPoint.X,
                box.MaxPoint.Y);
        }
        catch
        {
            return null;
        }
    }

    public static List<AnnotationBox> ReadAllBoxes(
        Sheet sheet,
        double viewPadding)
    {
        List<AnnotationBox> boxes =
            new();

        foreach (DrawingView view
                 in sheet.DrawingViews)
        {
            boxes.Add(
                new AnnotationBox(
                    $"view:{view.Name}",
                    "view",
                    view.Name,
                    view.Left - viewPadding,
                    view.Top - view.Height - viewPadding,
                    view.Left + view.Width + viewPadding,
                    view.Top + viewPadding));
        }

        GeneralDimensions dimensions =
            sheet
                .DrawingDimensions
                .GeneralDimensions;

        for (int index = 1;
             index <= dimensions.Count;
             index++)
        {
            dynamic dimension =
                dimensions[index];

            try
            {
                Box2d rangeBox =
                    (Box2d)dimension
                        .Text
                        .RangeBox;

                boxes.Add(
                    new AnnotationBox(
                        $"dimension:{index}",
                        "dimension",
                        ReadDimensionText(
                            dimension),
                        rangeBox.MinPoint.X,
                        rangeBox.MinPoint.Y,
                        rangeBox.MaxPoint.X,
                        rangeBox.MaxPoint.Y));
            }
            catch
            {
            }
        }

        HoleThreadNotes notes =
            sheet
                .DrawingNotes
                .HoleThreadNotes;

        for (int index = 1;
             index <= notes.Count;
             index++)
        {
            HoleThreadNote note =
                notes[index];

            AnnotationBox? box =
                TryReadBox(
                    $"hole_thread_note:{index}",
                    "hole_thread_note",
                    ReadHoleThreadNoteText(
                        note),
                    () =>
                        note.Text.RangeBox);

            if (box != null)
            {
                boxes.Add(
                    box);
            }
        }

        TryAddTitleBlockBox(
            sheet,
            boxes);

        return boxes;
    }

    public static string ReadDimensionText(
        dynamic dimension)
    {
        try
        {
            return
                (string)dimension
                    .Text
                    .Text;
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string ReadHoleThreadNoteText(
        HoleThreadNote note)
    {
        try
        {
            return note.Text.Text;
        }
        catch
        {
            return string.Empty;
        }
    }

    public static bool Intersects(
        AnnotationBox first,
        AnnotationBox second,
        double clearance)
    {
        return
            first.MinX - clearance <
                second.MaxX &&
            first.MaxX + clearance >
                second.MinX &&
            first.MinY - clearance <
                second.MaxY &&
            first.MaxY + clearance >
                second.MinY;
    }

    public static bool IsInsideSheet(
        AnnotationBox box,
        Sheet sheet,
        double margin)
    {
        return
            box.MinX >= margin &&
            box.MinY >= margin &&
            box.MaxX <=
                sheet.Width - margin &&
            box.MaxY <=
                sheet.Height - margin;
    }

    public static bool IsMovableAnnotationKind(
        string kind)
    {
        return
            string.Equals(
                kind,
                "dimension",
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                kind,
                "hole_thread_note",
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                kind,
                "leader_note",
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                kind,
                "surface_texture",
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                kind,
                "datum",
                StringComparison.OrdinalIgnoreCase);
    }

    public static object ToObject(
        AnnotationBox box)
    {
        return new
        {
            id =
                box.Id,

            kind =
                box.Kind,

            text =
                box.Text,

            minPoint =
                new
                {
                    x =
                        box.MinX,

                    y =
                        box.MinY
                },

            maxPoint =
                new
                {
                    x =
                        box.MaxX,

                    y =
                        box.MaxY
                },

            width =
                box.Width,

            height =
                box.Height,

            center =
                new
                {
                    x =
                        box.CenterX,

                    y =
                        box.CenterY
                }
        };
    }

    public static string CreateSuccess(
        object data)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    true,

                data
            },
            CreateJsonOptions());
    }

    public static string CreateError(
        string message,
        string? details = null)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    false,

                error =
                    message,

                details
            },
            CreateJsonOptions());
    }

    private static void TryAddTitleBlockBox(
        Sheet sheet,
        List<AnnotationBox> boxes)
    {
        try
        {
            dynamic titleBlock =
                sheet.TitleBlock;

            if (titleBlock == null)
            {
                return;
            }

            Box2d box =
                (Box2d)titleBlock.RangeBox;

            boxes.Add(
                new AnnotationBox(
                    "title_block:1",
                    "title_block",
                    titleBlock.Definition.Name,
                    box.MinPoint.X,
                    box.MinPoint.Y,
                    box.MaxPoint.X,
                    box.MaxPoint.Y));
        }
        catch
        {
        }
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented =
                true,

            Encoder =
                JavaScriptEncoder
                    .UnsafeRelaxedJsonEscaping
        };
    }

    internal sealed record AnnotationBox(
        string Id,
        string Kind,
        string Text,
        double MinX,
        double MinY,
        double MaxX,
        double MaxY)
    {
        public double Width =>
            MaxX - MinX;

        public double Height =>
            MaxY - MinY;

        public double CenterX =>
            (MinX + MaxX) / 2.0;

        public double CenterY =>
            (MinY + MaxY) / 2.0;
    }
}
