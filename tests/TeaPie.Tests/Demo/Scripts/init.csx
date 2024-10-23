#load "../FirstFolder/first.csx"

FirstMethod();

tp.Logger.LogInformation("I am user-defined script!");

public void Initialization()
{
    tp.Logger.LogInformation("Script was initialised!");
}
