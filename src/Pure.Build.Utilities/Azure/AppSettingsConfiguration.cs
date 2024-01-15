//
// Copyright (c) 2023-2024, Pure Software Ltd.  All rights reserved.
//
// Pure Software licenses this file to you under the following license(s):
//
//  * The MIT License, see https://opensource.org/license/mit/

using Nuke.Common.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Pure.Utilities.Azure;

public class AppSettingsConfiguration : IDisposable
{
    private readonly AbsolutePath _configFile;
    private readonly Serilog.ILogger _logger;
    private readonly JsonNode _jsonContent;
    private bool _disposed;

    public AppSettingsConfiguration(AbsolutePath configFile, Serilog.ILogger logger)
    {
        _configFile = configFile;
        _logger = logger;

        if (!File.Exists(configFile))
            throw new FileNotFoundException(configFile);

        var content = File.ReadAllText(configFile);

        _jsonContent = JsonNode.Parse(content, new JsonNodeOptions { PropertyNameCaseInsensitive = true }) ??
            throw new InvalidOperationException($"Unable to parse configuration file '{configFile}'");
    }

    public void UpdateValue(string section, string key, string value)
    {
        var jsonSection = _jsonContent[section] ??
            throw new ArgumentException($"Invalid section: '{section}'", nameof(section));

        jsonSection[key] = value;

        _logger.Information("Updated {file} entry {section} / {key}", _configFile, section, key);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                File.WriteAllText(_configFile, _jsonContent.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
