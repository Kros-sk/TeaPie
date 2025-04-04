using TeaPie.DotnetTool;

var app = ApplicationProvider.GetApplication();

Displayer.DisplayApplicationHeader();

return await app.RunAsync(args);
