namespace TeaPie.Reporting;

public interface IReporter
{
    Task Report();
}

public interface IReporter<TReportedObject>
{
    Task Report(TReportedObject report);
}
