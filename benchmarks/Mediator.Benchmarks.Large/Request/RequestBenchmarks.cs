using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Request;

public sealed record SomeRequest(Guid Id) : IRequest<SomeResponse>, MediatR.IRequest<SomeResponse>;

public sealed record SomeResponse(Guid Id);

public sealed class SomeHandlerClass
    : IRequestHandler<SomeRequest, SomeResponse>,
        MediatR.IRequestHandler<SomeRequest, SomeResponse>
{
    private static readonly SomeResponse _response = new SomeResponse(Guid.NewGuid());
    private static ValueTask<SomeResponse> _vtResponse => new ValueTask<SomeResponse>(_response);
    private static readonly Task<SomeResponse> _tResponse = Task.FromResult(_response);

    public ValueTask<SomeResponse> Handle(SomeRequest request, CancellationToken cancellationToken) => _vtResponse;

    Task<SomeResponse> MediatR.IRequestHandler<SomeRequest, SomeResponse>.Handle(
        SomeRequest request,
        CancellationToken cancellationToken
    ) => _tResponse;
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[RankColumn]
//[EventPipeProfiler(EventPipeProfile.CpuSampling)]
//[DisassemblyDiagnoser]
//[InliningDiagnoser(logFailuresOnly: true, allowedNamespaces: new[] { "Mediator" })]
public class RequestBenchmarks
{
    private IServiceProvider _serviceProvider;
    private IServiceScope _serviceScope;
    private IMediator _mediator;
    private Mediator _concreteMediator;
    private MediatR.IMediator _mediatr;
    private SomeHandlerClass _handler;
    private SomeRequest _request;

    [Params(Mediator.ServiceLifetime)]
    public ServiceLifetime ServiceLifetime { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddMediatR(opts =>
        {
            opts.Lifetime = Mediator.ServiceLifetime;
            opts.RegisterServicesFromAssembly(typeof(SomeHandlerClass).Assembly);
        });

        _serviceProvider = services.BuildServiceProvider();
#pragma warning disable CS0162 // Unreachable code detected
        if (Mediator.ServiceLifetime == ServiceLifetime.Scoped)
        {
            _serviceScope = _serviceProvider.CreateScope();
            _serviceProvider = _serviceScope.ServiceProvider;
        }
#pragma warning restore CS0162 // Unreachable code detected

        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        _concreteMediator = _serviceProvider.GetRequiredService<Mediator>();
        _mediatr = _serviceProvider.GetRequiredService<MediatR.IMediator>();
        _handler = _serviceProvider.GetRequiredService<SomeHandlerClass>();
        _request = new(Guid.NewGuid());
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (_serviceScope is not null)
            _serviceScope.Dispose();
        else
            (_serviceProvider as IDisposable)?.Dispose();
    }

    [Benchmark]
    public ValueTask<SomeResponse> SendRequest_IMediator()
    {
        return _mediator.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    public ValueTask<SomeResponse> SendRequest_Mediator()
    {
        return _concreteMediator.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    public Task<SomeResponse> SendRequest_MediatR()
    {
        return _mediatr.Send(_request, CancellationToken.None);
    }

    [Benchmark(Baseline = true)]
    public ValueTask<SomeResponse> SendRequest_Baseline()
    {
        return _handler.Handle(_request, CancellationToken.None);
    }
}
