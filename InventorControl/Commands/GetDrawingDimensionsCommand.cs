using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetDrawingDimensionsCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetDrawingDimensionsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_drawing_dimensions";

    public string Execute(
        JsonElement root)
    {
        Document? activeDocument =
            _inventor.ActiveDocument;

        if (activeDocument == null)
        {
            return CreateError(
                "В Inventor нет активного документа.");
        }

        if (activeDocument.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            return CreateError(
                "Активный документ не является чертежом.");
        }

        DrawingDocument drawingDocument =
            (DrawingDocument)activeDocument;

        Sheet activeSheet =
            drawingDocument.ActiveSheet;

        GeneralDimensions dimensions =
            activeSheet.DrawingDimensions
                .GeneralDimensions;

        List<object> result =
            new();

        int index =
            1;

        foreach (GeneralDimension dimension
                 in dimensions)
        {
            result.Add(
                CreateDimensionData(
                    dimension,
                    index));

            index++;
        }

        return CreateSuccess(
            new
            {
                document =
                    drawingDocument.DisplayName,

                sheet =
                    activeSheet.Name,

                dimensionCount =
                    result.Count,

                dimensions =
                    result
            });
    }

    private static object CreateDimensionData(
        GeneralDimension dimension,
        int index)
    {
        Point2d textOrigin =
            dimension.Text.Origin;

        Box2d textRange =
            dimension.Text.RangeBox;

        return new
        {
            index,

            objectType =
                dimension.Type
                    .ToString(),

            generalDimensionType =
                dimension.GeneralDimensionType
                    .ToString(),

            modelValue =
                dimension.ModelValue,

            displayedText =
                dimension.Text.Text,

            formattedText =
                dimension.Text.FormattedText,

            precision =
                dimension.Precision,

            attached =
                dimension.Attached,

            retrieved =
                dimension.Retrieved,

            hiddenValue =
                dimension.HideValue,

            modelValueOverridden =
                dimension.ModelValueOverridden,

            textPosition =
                new
                {
                    x =
                        textOrigin.X,

                    y =
                        textOrigin.Y
                },

            textRange =
                new
                {
                    minX =
                        textRange.MinPoint.X,

                    minY =
                        textRange.MinPoint.Y,

                    maxX =
                        textRange.MaxPoint.X,

                    maxY =
                        textRange.MaxPoint.Y
                },

            style =
                TryGetStyleName(
                    dimension)
        };
    }

    private static string?
        TryGetStyleName(
            GeneralDimension dimension)
    {
        try
        {
            return dimension.Style.Name;
        }
        catch
        {
            return null;
        }
    }

    private static string CreateSuccess(
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

    private static string CreateError(
        string message)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    false,

                error =
                    message
            },
            CreateJsonOptions());
    }

    private static JsonSerializerOptions
        CreateJsonOptions()
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
}