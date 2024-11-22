using TeaPie;

var builder = ApplicationBuilder.Create()
    .WithPath(args[0])
    .AddLogging();

var app = builder.Build();
await app.Run(new CancellationToken());
