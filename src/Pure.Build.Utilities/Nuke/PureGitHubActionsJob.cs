﻿//
// Copyright (c) 2023-2024, Pure Software Ltd.  All rights reserved.
//
// Pure Software licenses this file to you under the following license(s):
//
//  * The MIT License, see https://opensource.org/license/mit/

using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

namespace Pure.Build.Utilities.Nuke;

public class PureGitHubActionsJob : GitHubActionsJob
{
    public string[] Services { get; set; } = [];

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine($"{Name}:");

        using (writer.Indent())
        {
            writer.WriteLine($"name: {Name}");
            writer.WriteLine($"runs-on: {Image.GetValue()}");

            if (Services.Length > 0)
            {
                writer.WriteLine("services:");

                GitHubActionsServices.WriteYaml(Services[0], writer.WriteLine, writer.Indent);
            }

            if (TimeoutMinutes > 0)
            {
                writer.WriteLine($"timeout-minutes: {TimeoutMinutes}");
            }

            if (!ConcurrencyGroup.IsNullOrWhiteSpace() || ConcurrencyCancelInProgress)
            {
                writer.WriteLine("concurrency:");
                using (writer.Indent())
                {
                    var group = ConcurrencyGroup;
                    if (group.IsNullOrWhiteSpace())
                    {
                        // create a default value that only cancels in-progress runs of the same workflow
                        // we don't fall back to github.ref which would disable multiple runs in main/master which is usually what is wanted
                        group = "${{ github.workflow }} @ ${{ github.event.pull_request.head.label || github.head_ref || github.run_id }}";
                    }

                    writer.WriteLine($"group: {group}");
                    if (ConcurrencyCancelInProgress)
                    {
                        writer.WriteLine("cancel-in-progress: true");
                    }
                }
            }

            writer.WriteLine("steps:");
            using (writer.Indent())
            {
                Steps.ForEach(x => x.Write(writer));
            }
        }
    }
}
