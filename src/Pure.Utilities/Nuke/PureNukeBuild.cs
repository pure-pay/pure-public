//
// Copyright (c) 2023-2024, Pure Software Ltd.  All rights reserved.
//
// Pure Software licenses this file to you under the following license(s):
//
//  * The MIT License, see https://opensource.org/license/mit/

using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.MinVer;
using Nuke.Common.Tools.Slack;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using static Nuke.Common.Tools.Slack.SlackTasks;

namespace Pure.Utilities.Nuke;

public abstract class PureNukeBuild : NukeBuild
{
    protected readonly StringBuilder _updateText = new StringBuilder();

    [Parameter]
    [Secret]
    protected readonly string SlackWebhook = default!;

    [Solution]
    protected readonly Solution Solution = default!;

    [GitRepository]
    protected readonly GitRepository GitRepository = default!;

    [MinVer]
    protected readonly MinVer MinVer = default!;

    protected AbsolutePath SourceDirectory => RootDirectory / "src";

    protected AbsolutePath TestDirectory => RootDirectory / "test";

    protected AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    protected abstract string ProjectTitle { get; }

    protected override void OnBuildCreated()
    {
        _updateText.AppendLine($"Building *{ProjectTitle}*...");

        base.OnBuildCreated();
    }

    protected override void OnTargetFailed(string target)
    {
        _updateText.AppendLine($" • {target} failed");

        base.OnTargetFailed(target);
    }

    protected override void OnTargetSucceeded(string target)
    {
        _updateText.AppendLine($" • {target} succeeded");

        base.OnTargetSucceeded(target);
    }

    protected override void OnBuildFinished()
    {
        _updateText.AppendLine("Completed");

        NotifyBuildUpdate(_updateText.ToString());

        base.OnBuildFinished();
    }
    protected void NotifyBuildUpdate(string message)
    {
        if (SlackWebhook != null)
        {
            SendSlackMessage(_ => _
                    .SetText($"{DateTime.Now}: {message}"),
                $"https://hooks.slack.com/services/{SlackWebhook}");
        }
    }

    protected AbsolutePath CreateZipDeployment(AbsolutePath artifactsDirectory, AbsolutePath deploymentDirectory)
    {
        var zipFile = deploymentDirectory / "deployment.zip";

        if (File.Exists(zipFile))
            File.Delete(zipFile);

        if (!Directory.Exists(deploymentDirectory))
            Directory.CreateDirectory(deploymentDirectory);

        ZipFile.CreateFromDirectory(artifactsDirectory, zipFile);

        return zipFile;
    }
}