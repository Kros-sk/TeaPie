using TeaPie.Pipelines.Application;
using TeaPie.ScriptHandling;

namespace TeaPie.Pipelines.Scripts;

internal sealed class StoreScriptStep : IPipelineStep
{
    private readonly ScriptExecutionContext _script;

    private StoreScriptStep(ScriptExecutionContext script)
    {
        _script = script;
    }

    public static StoreScriptStep Create(ScriptExecutionContext script) => new(script);

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        if (_script.ProcessedContent is null)
        {
            throw new InvalidOperationException(
                "Processed content of the script can't be null when storing to temporary script file.");
        }

        var tmpPath = Path.Combine(context.TempFolderPath, _script.Script.File.RelativePath);

        var parent = Directory.GetParent(tmpPath);
        ArgumentNullException.ThrowIfNull(parent);

        if (!Directory.Exists(parent.FullName))
        {
            Directory.CreateDirectory(parent.FullName);
        }

        await File.WriteAllTextAsync(tmpPath, _script.ProcessedContent, cancellationToken);
    }
}
