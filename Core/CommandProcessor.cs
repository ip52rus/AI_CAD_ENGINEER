namespace AI_CAD_ENGINEER.Core;

public class CommandProcessor
{
    private readonly InventorManager _inventorManager;

    public CommandProcessor(InventorManager inventorManager)
    {
        _inventorManager = inventorManager;
    }

    public void Process(string? command)
    {
        if (!string.Equals(
                command,
                "Создай чертежи",
                StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Неизвестная команда.");
            return;
        }

        if (_inventorManager.GetActiveDocument() == null)
        {
            Console.WriteLine("В Inventor нет открытого документа.");
            return;
        }

        if (!_inventorManager.IsPart())
        {
            Console.WriteLine(
                "Команда пока поддерживается только для деталей.");
            return;
        }

        Inventor.Application? inventor =
            _inventorManager.GetInventorApplication();

        if (inventor == null)
        {
            Console.WriteLine("Подключение к Inventor отсутствует.");
            return;
        }

        DrawingManager drawingManager = new(inventor);

        if (drawingManager.CreateEmptyDrawing())
        {
            Console.WriteLine("Пустой чертёж успешно создан.");
        }
        else
        {
            Console.WriteLine("Не удалось создать пустой чертёж.");
        }
    }
}