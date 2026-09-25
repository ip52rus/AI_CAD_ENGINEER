using AI_CAD_ENGINEER.Core;

namespace AI_CAD_ENGINEER;

internal static class Program
{
    [STAThread]
    private static int Main(
        string[] args)
    {
        Application application =
            new();

        return application.Run(
            args);
    }
}
