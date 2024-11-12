namespace TeaPie.Exceptions;

internal class NuGetPackageNotFoundException : Exception
{
    public NuGetPackageNotFoundException(string packageName, string version)
        : base($"The NuGet package '{packageName}' version '{version}' could not be found.")
    {
    }

    public NuGetPackageNotFoundException()
    {
    }

    public NuGetPackageNotFoundException(string? message) : base(message)
    {
    }

    public NuGetPackageNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
