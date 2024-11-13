using TeaPie.Pipelines.Application;
using TeaPie.Requests;

namespace TeaPie.Pipelines.Requests;

internal class ExecuteRequestStep(Client client) : IPipelineStep
{
    private readonly Client _httpClient = client;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }
}
