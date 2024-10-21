namespace TeaPie.Exceptions;

internal class NugetPackageNotFoundException : Exception
{
    public NugetPackageNotFoundException(string packageName, string version)
        : base($"The NuGet package '{packageName}' version '{version}' could not be found.")
    {
    }

    public NugetPackageNotFoundException()
    {
    }

    public NugetPackageNotFoundException(string? message) : base(message)
    {
    }

    public NugetPackageNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
