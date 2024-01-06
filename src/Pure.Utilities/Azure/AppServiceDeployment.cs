﻿//
// Copyright (c) 2023-2024, Pure Software Ltd.  All rights reserved.
//
// Pure Software licenses this file to you under the following license(s):
//
//  * The MIT License, see https://opensource.org/license/mit/

using System.Text;

namespace Pure.Utilities.Azure;

public class AppServiceDeployment
{
    public static async Task DeployAsync(string zipFilePath, string appServiceName, string username, string password)
    {
        var base64Auth = Convert.ToBase64String(Encoding.Default.GetBytes($"{username}:{password}"));

        byte[] fileContents = File.ReadAllBytes(zipFilePath);

        using var memStream = new MemoryStream(fileContents);

        memStream.Position = 0;

        var content = new StreamContent(memStream);

        var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64Auth);

        var requestUrl = $"https://{appServiceName}.scm.azurewebsites.net/api/zipdeploy";

        var response = await httpClient.PostAsync(requestUrl, content);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
    }
}