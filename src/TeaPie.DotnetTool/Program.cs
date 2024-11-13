using Microsoft.Extensions.DependencyInjection;
using TeaPie.Extensions;
using TeaPie.Parsing;
using TeaPie.Pipelines;
using TeaPie.Pipelines.Application;
using TeaPie.Requests;

var services = new ServiceCollection();
services.ConfigureServices();
services.ConfigureLogging();
services.ConfigureAccessors();
services.ConfigureHttpClient();
services.AddSteps();
services.AddSingleton<IPipeline, ApplicationPipeline>();
services.AddSingleton<Client>();

var provider = services.BuildServiceProvider();

// This section will be part of different class, now it is here just for testing purposes
//if (args.Length > 0)
//{
//    TeaPie.TeaPie.Create(provider.GetRequiredService<ILogger<TeaPie.TeaPie>>());

//    var pipeline = provider.GetRequiredService<IPipeline>();
//    pipeline.AddSteps(provider.GetStep<StructureExplorationStep>());
//    pipeline.AddSteps(provider.GetStep<PrepareTemporaryFolderStep>());
//    pipeline.AddSteps(provider.GetStep<StepsGenerationStep>());

//    var context = new ApplicationContext(args[0], provider.GetRequiredService<ILogger<ApplicationContext>>(), provider);

//    await pipeline.Run(context, CancellationToken.None);
//}

var http = File.ReadAllText("C:\\Projects\\Topics\\Diploma thesis\\Draft\\TeaPieDraft\\TeaPieDraft\\Scripts\\Tests1\\Test1-req.http");
var results = HttpFileParser.ParseHttpFile(http);

var client = provider.GetRequiredService<Client>();
await client.SendRequest(results);
