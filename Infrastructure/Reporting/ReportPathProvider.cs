namespace AI_CAD_ENGINEER.Infrastructure.Reporting;

public class ReportPathProvider
{
    private readonly string _reportsRootDirectory;

    public ReportPathProvider()
    {
        _reportsRootDirectory =
            Path.Combine(
                AppContext.BaseDirectory,
                "Reports");
    }

    public string CreateReportPath(
        ReportCategory category,
        string reportName)
    {
        if (string.IsNullOrWhiteSpace(reportName))
        {
            throw new ArgumentException(
                "Имя отчёта не может быть пустым.",
                nameof(reportName));
        }

        string categoryDirectory =
            Path.Combine(
                _reportsRootDirectory,
                category.ToString());

        Directory.CreateDirectory(
            categoryDirectory);

        string safeReportName =
            MakeSafeFileName(reportName);

        string timestamp =
            DateTime.Now.ToString(
                "yyyy-MM-dd_HH-mm-ss-fff");

        string fileName =
            $"{safeReportName}_{timestamp}.txt";

        return Path.Combine(
            categoryDirectory,
            fileName);
    }

    private static string MakeSafeFileName(
        string fileName)
    {
        char[] invalidCharacters =
            Path.GetInvalidFileNameChars();

        string safeFileName =
            new(
                fileName
                    .Select(character =>
                        invalidCharacters.Contains(character)
                            ? '_'
                            : character)
                    .ToArray());

        return safeFileName;
    }
}