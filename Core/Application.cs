using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using AI_CAD_ENGINEER.InventorControl;
using Inventor;

namespace AI_CAD_ENGINEER.Core;

public class Application
{
    public int Run(
        string[] args)
    {
        Console.OutputEncoding =
            new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: false);

        if (TryGetSingleCommandJson(
                args,
                out string? commandJson,
                out string? argumentError))
        {
            return RunSingleCommand(
                commandJson!);
        }

        if (argumentError != null)
        {
            Console.WriteLine(
                CreateErrorJson(
                    argumentError));

            return 2;
        }

        RunInteractive();

        return 0;
    }

    private static bool TryGetSingleCommandJson(
        string[] args,
        out string? commandJson,
        out string? error)
    {
        commandJson = null;
        error = null;

        if (args.Length == 0)
        {
            return false;
        }

        if (args.Length == 2 &&
            string.Equals(
                args[0],
                "--json",
                StringComparison.OrdinalIgnoreCase))
        {
            commandJson =
                args[1];

            if (string.IsNullOrWhiteSpace(
                    commandJson))
            {
                error =
                    "--json argument must not be empty.";

                return false;
            }

            return true;
        }

        if (args.Length == 2 &&
            string.Equals(
                args[0],
                "--json-file",
                StringComparison.OrdinalIgnoreCase))
        {
            string path =
                args[1];

            if (string.IsNullOrWhiteSpace(
                    path))
            {
                error =
                    "--json-file path must not be empty.";

                return false;
            }

            try
            {
                commandJson =
                    System.IO.File.ReadAllText(
                        path,
                        Encoding.UTF8);
            }
            catch (Exception exception)
            {
                error =
                    $"Unable to read --json-file: {exception.Message}";

                return false;
            }

            if (string.IsNullOrWhiteSpace(
                    commandJson))
            {
                error =
                    "--json-file must contain one JSON command.";

                return false;
            }

            return true;
        }

        error =
            "Invalid arguments. Use either no arguments for interactive mode, --json \"<JSON>\", or --json-file \"<path>\".";

        return false;
    }

    private static int RunSingleCommand(
        string commandJson)
    {
        try
        {
            using (JsonDocument.Parse(
                       commandJson))
            {
            }
        }
        catch (JsonException exception)
        {
            Console.WriteLine(
                CreateErrorJson(
                    "Некорректный JSON.",
                    exception.Message));

            return 1;
        }

        InventorManager inventorManager =
            new();

        if (!inventorManager.AttachToRunningInventor())
        {
            Console.WriteLine(
                CreateErrorJson(
                    "No running Autodesk Inventor instance could be attached."));

            return 1;
        }

        Inventor.Application? inventor =
            inventorManager.GetInventorApplication();

        if (inventor == null)
        {
            Console.WriteLine(
                CreateErrorJson(
                    "No running Autodesk Inventor instance could be attached."));

            return 1;
        }

        InventorCommandDispatcher dispatcher =
            new(
                inventor);

        string response =
            dispatcher.Execute(
                commandJson);

        Console.WriteLine(
            response);

        return IsSuccessResponse(
            response)
            ? 0
            : 1;
    }

    private static bool IsSuccessResponse(
        string response)
    {
        try
        {
            using JsonDocument document =
                JsonDocument.Parse(
                    response);

            if (document.RootElement.TryGetProperty(
                    "success",
                    out JsonElement successElement) &&
                successElement.ValueKind ==
                    JsonValueKind.True)
            {
                return true;
            }
        }
        catch
        {
        }

        return false;
    }

    private static string CreateErrorJson(
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
            new JsonSerializerOptions
            {
                WriteIndented =
                    true,

                Encoder =
                    JavaScriptEncoder
                        .UnsafeRelaxedJsonEscaping
            });
    }

    private void RunInteractive()
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
            "      AI CAD ENGINEER v0.68");

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
