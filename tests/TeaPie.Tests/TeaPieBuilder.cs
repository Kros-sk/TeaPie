using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Http.Auth;
using TeaPie.Http.Retrying;
using TeaPie.Pipelines;
using TeaPie.Reporting;
using TeaPie.TestCases;
using TeaPie.Testing;
using TeaPie.Variables;

namespace TeaPie.Tests;

internal class TeaPieBuilder
{
    private readonly ServiceCollection _serviceCollection;
    private ApplicationContext _appContext;

    public TeaPieBuilder()
    {
        _serviceCollection = new ServiceCollection();
        _appContext = new ApplicationContextBuilder().Build();

        _serviceCollection.AddSingleton(Substitute.For<IVariables>());
        _serviceCollection.AddSingleton(Substitute.For<ILogger<TeaPie>>());
        _serviceCollection.AddSingleton(Substitute.For<ITester>());
        _serviceCollection.AddSingleton(Substitute.For<ICurrentTestCaseExecutionContextAccessor>());
        _serviceCollection.AddSingleton(Substitute.For<IPipeline>());
        _serviceCollection.AddSingleton(Substitute.For<ITestResultsSummaryReporter>());
        _serviceCollection.AddSingleton(Substitute.For<IAuthProviderRegistry>());
        _serviceCollection.AddSingleton(Substitute.For<ICurrentAndDefaultAuthProviderAccessor>());
        _serviceCollection.AddSingleton(Substitute.For<IRetryStrategyRegistry>());
    }

    public TeaPieBuilder WithService<TService>(TService implementation) where TService : class
    {
        var descriptor = _serviceCollection.FirstOrDefault(d => d.ServiceType == typeof(TService));
        if (descriptor != null)
        {
            _serviceCollection.Remove(descriptor);
        }

        _serviceCollection.AddSingleton(implementation);
        return this;
    }

    public TeaPie Build()
    {
        var provider = _serviceCollection.BuildServiceProvider();
        return TeaPie.Create(_appContext, provider);
    }

    public TeaPieBuilder WithApplicationContext(ApplicationContext appContext)
    {
        _appContext = appContext;
        return this;
    }
}
