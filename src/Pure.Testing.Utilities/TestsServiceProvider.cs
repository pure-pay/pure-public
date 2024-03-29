﻿//
// Copyright (c) 2023-2024, Pure Software Ltd.  All rights reserved.
//
// Pure Software licenses this file to you under the following license(s):
//
//  * The MIT License, see https://opensource.org/license/mit/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Pure.Testing.Utilities;

public class TestsServiceProvider : IServiceProvider
{
    private readonly HostApplicationBuilder _hostBuilder;
    private readonly IServiceCollection _services;
    private IHost? _host;

    private IServiceProvider Services => _host?.Services ??
        throw new InvalidOperationException("Service host not initialised; call Build() before any calls to GetService()");

    public TestsServiceProvider()
    {
        _hostBuilder = new HostApplicationBuilder();
        _services = _hostBuilder.Services;

        _services.AddLogging();
    }

    public void Build() => _host = _hostBuilder.Build();

    public IServiceCollection AddSingleton<TService>(TService implementationInstance) where TService : class =>
        _host == null ? _services.AddSingleton(implementationInstance) :
            throw new InvalidOperationException("All services must be added before Build() is called");

    public IServiceCollection AddSingleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService =>
        _host == null ? _services.AddSingleton<TService, TImplementation>() :
            throw new InvalidOperationException("All services must be added before Build() is called");

    public IServiceCollection AddSingleton<TService>(Func<IServiceProvider, TService> implementationFactory)
        where TService : class =>
        _host == null ? _services.AddSingleton<TService>(implementationFactory) :
            throw new InvalidOperationException("All services must be added before Build() is called");

    public TService GetRequiredService<TService>() where TService : class =>
        Services.GetRequiredService<TService>();

    public TService? GetService<TService>() => Services.GetService<TService>();

    public object? GetService(Type serviceType) => Services.GetService(serviceType);

    public ILogger<T> GetLogger<T>() where T : class =>
        Services.GetService<ILoggerFactory>()?.CreateLogger<T>() ??
            throw new InvalidOperationException("Unable to create logger");
}