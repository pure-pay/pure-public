//
// Copyright (c) 2023-2024, Pure Software Ltd.  All rights reserved.
//
// Pure Software licenses this file to you under the following license(s):
//
//  * The MIT License, see https://opensource.org/license/mit/

using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Pure.Utilities.Nuke;
using System.Linq;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[GitHubActions(
    "continuous",
    GitHubActionsImage.UbuntuLatest,
    On = new[] { GitHubActionsTrigger.Push },
    InvokedTargets = new[] { nameof(Publish) },
    ImportSecrets = new[] { nameof(NugetApiKey), nameof(SlackWebhook) })]
class Build : PureNukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter]
    readonly string NugetApiUrl = "https://api.nuget.org/v3/index.json"; //default

    [Parameter]
    [Secret]
    readonly string NugetApiKey;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    protected override string ProjectTitle => "Pure.Public";

    Target Clean => _ => _
        .Requires(() => SlackWebhook)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(d => d.DeleteDirectory());
            TestDirectory.GlobDirectories("**/bin", "**/obj").ForEach(d => d.DeleteDirectory());
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(MinVer.Version)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .EnableNoBuild());
        });

    Target Pack => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetIncludeSymbols(true)
                .SetOutputDirectory(ArtifactsDirectory)
                .EnableNoRestore()
                .EnableNoBuild());
        });

    Target Publish => _ => _
        .OnlyWhenDynamic(() => GitRepository.Tags.Any())
        .Requires(() => NugetApiKey)
        .Requires(() => NugetApiUrl)
        .DependsOn(Pack)
        .Executes(() =>
        {
            _slackUpdate.AppendLine($"  ->  Publishing package(s) as version {GitRepository.Tags.Last()}");

            var packageFiles = ArtifactsDirectory
                .GlobFiles("*.nupkg")
                .Where(x => !x.ToString().EndsWith("symbols.nupkg"))
                .ToList();

            packageFiles.ForEach(p =>
                DotNetNuGetPush(s => s
                    .SetTargetPath(p)
                    .SetSource(NugetApiUrl)
                    .SetApiKey(NugetApiKey)
                ));
        });
}