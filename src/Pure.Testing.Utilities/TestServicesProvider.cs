//
// Copyright (c) 2023-2024, Pure Software Ltd.  All rights reserved.
//
// Pure Software licenses this file to you under the following license(s):
//
//  * The MIT License, see https://opensource.org/license/mit/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Pure.Testing.Utilities;

public class TestServicesProvider
{
    public static ILogger<T> GetLogger<T>() where T : class
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        var factory = serviceProvider.GetService<ILoggerFactory>();

        return factory?.CreateLogger<T>() ??
            throw new InvalidOperationException("Unable to create logger");
    }
}
