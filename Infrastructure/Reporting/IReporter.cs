namespace AI_CAD_ENGINEER.Infrastructure.Reporting;

public interface IReporter
{
    string Write(
        ReportCategory category,
        string reportName,
        string report);
}