using Microsoft.Extensions.DependencyInjection;
using TeaPie;

var services = new ServiceCollection();
services.ConfigureServices();
var provider = services.BuildServiceProvider();
