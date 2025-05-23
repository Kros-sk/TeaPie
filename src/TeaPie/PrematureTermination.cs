﻿namespace TeaPie;

internal record PrematureTermination(
    string Source, TerminationType Type, string Details = "", int ExitCode = (int)ExitCode.Success);

internal enum TerminationType
{
    UserAction = 0,
    ApplicationError = 1,
    Canceled = 130
}
