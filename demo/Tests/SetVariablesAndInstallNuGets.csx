#nuget "FluentAssertions, 6.12.1"

public void SetVariables()
{
    tp.SetVariable("ApiBaseUrl", "http://localhost:3001");
    tp.SetVariable("ApiCarsSection", "/cars");
}