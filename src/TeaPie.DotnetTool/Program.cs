using Microsoft.Extensions.DependencyInjection;
using TeaPie;
using TeaPie.Extensions;

var services = new ServiceCollection();
services.ConfigureServices();
services.ConfigureLogging();
services.ConfigureAccessors();
services.ConfigureSteps();
services.AddSingleton<IPipeline, ApplicationPipeline>();

var provider = services.BuildServiceProvider();
