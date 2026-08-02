namespace AI_CAD_ENGINEER.Core;

public class Application
{
    public void Run()
    {
        Console.WriteLine("=================================");
        Console.WriteLine("      AI CAD ENGINEER v0.1");
        Console.WriteLine("=================================");
        Console.WriteLine();

        InventorManager inventorManager = new();

        if (!inventorManager.Connect())
        {
            Console.WriteLine("Не удалось подключиться к Autodesk Inventor.");
            Console.WriteLine();
            Console.WriteLine("Нажмите Enter для выхода...");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("Успешно подключились к Autodesk Inventor.");
        Console.WriteLine();

        if (inventorManager.GetActiveDocument() == null)
        {
            Console.WriteLine("В Inventor нет открытого документа.");
        }
        else
        {
            Console.WriteLine(
                $"Активный документ: {inventorManager.GetDocumentName()}");

            if (inventorManager.IsPart())
            {
                Console.WriteLine("Тип документа: Деталь");
            }
            else if (inventorManager.IsAssembly())
            {
                Console.WriteLine("Тип документа: Сборка");
            }
            else if (inventorManager.IsDrawing())
            {
                Console.WriteLine("Тип документа: Чертёж");
            }
            else
            {
                Console.WriteLine("Тип документа: Другой");
            }
        }

        Console.WriteLine();
        Console.Write("Введите команду: ");

        string? command = Console.ReadLine();

        Console.WriteLine();

        CommandProcessor processor = new(inventorManager);
        processor.Process(command);

        Console.WriteLine();
        Console.WriteLine("Нажмите Enter для выхода...");
        Console.ReadLine();
    }
}