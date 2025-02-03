namespace TeaPie.Reporting;

public interface ICompositeReporter<TReporterType, TReportedObject> : IReporter
    where TReporterType : IReporter<TReportedObject>
{
    void RegisterReporter(TReporterType reporter);

    void UnregisterReporter(TReporterType reporter);
}
