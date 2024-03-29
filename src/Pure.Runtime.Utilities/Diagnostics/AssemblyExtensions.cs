﻿//
// Copyright (c) 2023-2024, Pure Software Ltd.  All rights reserved.
//
// Pure Software licenses this file to you under the following license(s):
//
//  * The MIT License, see https://opensource.org/license/mit/

using System.Diagnostics;
using System.Reflection;

namespace Pure.Runtime.Utilities.Diagnostics;

public static class AssemblyExtensions
{
    public static DateTime GetBuildTimestamp(this Assembly assembly) =>
        new FileInfo(assembly.Location).LastWriteTime;

    public static string GetFileVersion(this Assembly assembly) =>
        FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion ?? "0.0.0.0";
}