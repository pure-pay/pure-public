//
// Copyright (c) 2023-2024, Pure Software Ltd.  All rights reserved.
//
// Pure Software licenses this file to you under the following license(s):
//
//  * The MIT License, see https://opensource.org/license/mit/

using FluentAssertions;
using Pure.Utilities.Diagnostics;
using System.Reflection;

namespace Pure.Utilities.Tests;

public class AssemblyExtensionsTests
{
    [Fact]
    public void CheckBuildTime()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var timestamp = assembly.GetBuildTimestamp();

        timestamp.Should().BeCloseTo(DateTime.Now, new TimeSpan(0,1,0));
    }
}