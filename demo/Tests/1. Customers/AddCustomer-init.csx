// Use 'load' directive to reference another script.
// Path can be absolute or relative (relative path starts from the parent directory).
// IMPORTANT: Referenced scripts run automatically. Encapsulate logic in methods to avoid unintended execution.
#load ../InitializationScript.csx

// Call a function defined in the referenced script.
Initialize();

// Logger implementing Microsoft's ILogger is accessible everywhere in the scripts.
tp.Logger.LogInformation("Start of demo collection testing.");
