// Reference another scripts using the 'load' directives.
// Paths can be absolute or relative (relative paths start from the script's parent directory).
#load ../InitializationScript.csx

// Call a function defined in the referenced script.
SetVariables();

// Logger implementing Microsoft's ILogger is accessible everywhere in the scripts.
tp.Logger.LogInformation("Start of demo collection testing.");
