﻿namespace TeaPie.Pipelines;

internal interface IPipelineStep
{
    Task Execute(ApplicationContext context, CancellationToken cancellationToken = default);
}
