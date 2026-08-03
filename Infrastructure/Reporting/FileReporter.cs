using System.Text;

namespace AI_CAD_ENGINEER.Infrastructure.Reporting;

public class FileReporter : IReporter
{
    private readonly ReportPathProvider _pathProvider;

    public FileReporter()
    {
        _pathProvider =
            new ReportPathProvider();
    }

    public string Write(
        ReportCategory category,
        string reportName,
        string report)
    {
        if (string.IsNullOrWhiteSpace(report))
        {
            throw new ArgumentException(
                "Текст отчёта не может быть пустым.",
                nameof(report));
        }

        string reportPath =
            _pathProvider.CreateReportPath(
                category,
                reportName);

        File.WriteAllText(
            reportPath,
            report,
            new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: true));

        return reportPath;
    }
}