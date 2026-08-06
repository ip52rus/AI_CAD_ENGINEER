using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetAssemblyReferencedDocumentsCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetAssemblyReferencedDocumentsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "get_assembly_referenced_documents";

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

        List<object> documents =
            AssemblyReadSupport
                .ReadReferencedDocuments(
                    assembly);

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

                    referencedDocumentCount =
                        documents.Count,

                    referencedDocuments =
                        documents
                });
    }
}
