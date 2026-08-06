using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetAssemblyOccurrencesCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetAssemblyOccurrencesCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "get_assembly_occurrences";

    public string Execute(
        JsonElement root)
    {
        AssemblyDocument? assembly =
            AssemblyReadSupport
                .ResolveAssemblyDocument(
                    _inventor,
                    root,
                    out DrawingDocument? drawing,
                    out Sheet? sheet,
                    out DrawingView? view,
                    out string? error);

        if (assembly == null)
        {
            return AssemblyReadSupport
                .CreateError(
                    error ??
                    "Не удалось получить сборку.");
        }

        int maxDepth =
            AssemblyReadSupport
                .GetOptionalInt32(
                    root,
                    "maxDepth",
                    -1);

        bool includeSuppressed =
            AssemblyReadSupport
                .GetOptionalBoolean(
                    root,
                    "includeSuppressed",
                    true);

        List<object> occurrences =
            AssemblyReadSupport
                .ReadOccurrences(
                    assembly,
                    maxDepth,
                    includeSuppressed);

        return AssemblyReadSupport
            .CreateSuccess(
                new
                {
                    drawing =
                        drawing?.DisplayName,

                    sheet =
                        sheet?.Name,

                    view =
                        view?.Name,

                    assemblyDocument =
                        AssemblyReadSupport
                            .ReadDocument(
                                assembly),

                    maxDepth,
                    includeSuppressed,

                    occurrenceCount =
                        occurrences.Count,

                    occurrences
                });
    }
}
