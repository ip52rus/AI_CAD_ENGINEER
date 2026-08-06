using AI_CAD_ENGINEER.InventorControl;
using Inventor;

namespace AI_CAD_ENGINEER.Core;

public class Application
{
    public void Run()
    {
        PrintHeader();

        InventorManager inventorManager =
            new();

        if (!inventorManager.Connect())
        {
            Console.WriteLine(
                "Не удалось подключиться к Autodesk Inventor.");

            WaitForExit();

            return;
        }

        Console.WriteLine(
            "Успешно подключились к Autodesk Inventor.");

        
        Inventor.Application? inventor =
            inventorManager.GetInventorApplication();

        if (inventor == null)
        {
            Console.WriteLine(
                "Не удалось получить экземпляр Inventor.Application.");

            WaitForExit();

            return;
        }

        PrintActiveDocument(
            inventorManager);

        InventorCommandDispatcher dispatcher =
            new(
                inventor);

        RunCommandLoop(
            dispatcher);
    }

    private static void RunCommandLoop(
        InventorCommandDispatcher dispatcher)
    {
        Console.WriteLine();
        Console.WriteLine(
            "Режим управления Inventor через JSON-команды.");

        Console.WriteLine();
        Console.WriteLine(
            "Доступные команды:");

        Console.WriteLine(
            """{"command":"ping"}""");

        Console.WriteLine(
            """{"command":"get_active_document"}""");

        Console.WriteLine(
            """{"command":"update_active_document"}""");

        Console.WriteLine();
        Console.WriteLine(
            "Для выхода введи:");

        Console.WriteLine(
            "exit");

        while (true)
        {
            Console.WriteLine();
            Console.Write(
                "JSON-команда: ");

            string? commandJson =
                Console.ReadLine();

            if (string.Equals(
                    commandJson?.Trim(),
                    "exit",
                    StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(
                    commandJson))
            {
                Console.WriteLine(
                    "Команда не введена.");

                continue;
            }

            string result =
                dispatcher.Execute(
                    commandJson);

            Console.WriteLine();
            Console.WriteLine(
                result);
        }

        Console.WriteLine();
        Console.WriteLine(
            "Работа завершена.");
    }

    private static void PrintActiveDocument(
        InventorManager inventorManager)
    {
        Document? activeDocument =
            inventorManager.GetActiveDocument();

        Console.WriteLine();

        if (activeDocument == null)
        {
            Console.WriteLine(
                "В Inventor нет открытого документа.");

            return;
        }

        Console.WriteLine(
            $"Активный документ: " +
            $"{inventorManager.GetDocumentName()}");

        if (inventorManager.IsPart())
        {
            Console.WriteLine(
                "Тип документа: Деталь");
        }
        else if (inventorManager.IsAssembly())
        {
            Console.WriteLine(
                "Тип документа: Сборка");
        }
        else if (inventorManager.IsDrawing())
        {
            Console.WriteLine(
                "Тип документа: Чертёж");
        }
        else
        {
            Console.WriteLine(
                "Тип документа: Другой");
        }
    }

    private static void PrintHeader()
    {
        Console.WriteLine(
            "=================================");

        Console.WriteLine(
            "      AI CAD ENGINEER v0.14");

        Console.WriteLine(
            "=================================");

        Console.WriteLine();
    }

    private static void WaitForExit()
    {
        Console.WriteLine();
        Console.WriteLine(
            "Нажмите Enter для выхода...");

        Console.ReadLine();
    }
}