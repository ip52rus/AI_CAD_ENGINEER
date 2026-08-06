using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetApplicationAddInsCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetApplicationAddInsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_application_addins";

    public string Execute(
        JsonElement root)
    {
        bool onlyGostRelated =
            true;

        if (root.TryGetProperty(
                "onlyGostRelated",
                out JsonElement filterElement))
        {
            if (filterElement.ValueKind !=
                JsonValueKind.True &&
                filterElement.ValueKind !=
                JsonValueKind.False)
            {
                return AddInDiagnosticSupport
                    .CreateError(
                        "Поле \"onlyGostRelated\" " +
                        "должно содержать true или false.");
            }

            onlyGostRelated =
                filterElement.GetBoolean();
        }

        List<object> addIns =
            new();

        int sourceIndex =
            1;

        foreach (ApplicationAddIn addIn
                 in _inventor.ApplicationAddIns)
        {
            string displayName =
                AddInDiagnosticSupport
                    .SafeGetString(
                        () => addIn.DisplayName);

            string description =
                AddInDiagnosticSupport
                    .SafeGetString(
                        () => addIn.Description);

            string classId =
                AddInDiagnosticSupport
                    .SafeGetString(
                        () => addIn.ClassIdString);

            string location =
                AddInDiagnosticSupport
                    .SafeGetString(
                        () => addIn.Location);

            bool looksLikeGost =
                AddInDiagnosticSupport
                    .LooksLikeGostAddIn(
                        displayName,
                        description,
                        classId,
                        location);

            if (onlyGostRelated &&
                !looksLikeGost)
            {
                sourceIndex++;
                continue;
            }

            string automationType =
                AddInDiagnosticSupport
                    .SafeGetAutomationType(
                        addIn,
                        out bool automationAvailable,
                        out string? automationError);

            addIns.Add(
                new
                {
                    sourceIndex,

                    displayName,

                    description,

                    classId,

                    location,

                    activated =
                        AddInDiagnosticSupport
                            .SafeGetBoolean(
                                () => addIn.Activated),

                    loadAutomatically =
                        AddInDiagnosticSupport
                            .SafeGetBoolean(
                                () => addIn.LoadAutomatically),

                    userUnloadable =
                        AddInDiagnosticSupport
                            .SafeGetBoolean(
                                () => addIn.UserUnloadable),

                    looksLikeGost,

                    automationAvailable,

                    automationType,

                    automationError
                });

            sourceIndex++;
        }

        return AddInDiagnosticSupport
            .CreateSuccess(
                new
                {
                    onlyGostRelated,

                    totalApplicationAddInCount =
                        _inventor.ApplicationAddIns.Count,

                    returnedAddInCount =
                        addIns.Count,

                    addIns
                });
    }
}
