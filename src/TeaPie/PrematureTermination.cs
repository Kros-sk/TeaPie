namespace TeaPie;

internal record PrematureTermination(string Source, TerminationType Type, string Details = "");

internal enum TerminationType
{
    User = 0,
    ApplicationError = 1,
    Canceled = 130
}
