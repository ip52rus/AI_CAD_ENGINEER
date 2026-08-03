using AI_CAD_ENGINEER.Drawing;
using AI_CAD_ENGINEER.Engineering.Models;
using AI_CAD_ENGINEER.Import.Inventor;

namespace AI_CAD_ENGINEER.Core;

public class CommandProcessor
{
    private readonly InventorManager _inventorManager;

    public CommandProcessor(
        InventorManager inventorManager)
    {
        _inventorManager =
            inventorManager;
    }

    public void Process(
        string? command)
    {
        if (!string.Equals(
                command,
                "Создай чертежи",
                StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(
                "Неизвестная команда.");

            return;
        }

        global::Inventor.Document? activeDocument =
            _inventorManager.GetActiveDocument();

        if (activeDocument == null)
        {
            Console.WriteLine(
                "В Inventor нет открытого документа.");

            return;
        }

        if (!_inventorManager.IsPart())
        {
            Console.WriteLine(
                "Команда пока поддерживается " +
                "только для деталей.");

            return;
        }

        global::Inventor.Application? inventor =
            _inventorManager.GetInventorApplication();

        if (inventor == null)
        {
            Console.WriteLine(
                "Подключение к Inventor отсутствует.");

            return;
        }

        PartAnalysis partAnalysis;

        try
        {
            ModelAnalyzer modelAnalyzer =
                new();

            partAnalysis =
                modelAnalyzer.Analyze(
                    activeDocument);

            modelAnalyzer.PrintReport(
                partAnalysis);
        }
        catch (Exception exception)
        {
            Console.WriteLine();
            Console.WriteLine(
                "Не удалось выполнить анализ 3D-модели.");

            Console.WriteLine(
                $"Причина: {exception.Message}");

            return;
        }

        DrawingManager drawingManager =
            new(inventor);

        bool drawingCreated =
            drawingManager.CreateDrawingWithViews(
                activeDocument,
                partAnalysis);

        if (drawingCreated)
        {
            Console.WriteLine(
                "Чертёж с тремя видами " +
                "успешно создан.");
        }
        else
        {
            Console.WriteLine(
                "Не удалось создать чертёж " +
                "с тремя видами.");
        }
    }
}