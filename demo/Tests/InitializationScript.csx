public void SetEnvironment()
{
    // By default, environments are defined in a <collection-name>-env.json file.
    // Use the option '--env-file <path-to-environment-file>' to specify a custom environment file.
    // If no environment file is found or specified, the collection runs without an environment.
    // The default environment ('$shared') is used if no specific environment is set.
    // Environments can be switched dynamically at runtime.
    tp.SetEnvironment("local");
}
