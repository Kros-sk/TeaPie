using Microsoft.Extensions.DependencyInjection;
using TeaPie;

var services = new ServiceCollection();
services.ConfigureServices();
services.ConfigureLogging();
var provider = services.BuildServiceProvider();
