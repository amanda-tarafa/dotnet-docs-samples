/*
Copyright 2020 Google LLC

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    https://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

public class CloudRunClient
{
    /// <summary>
    /// Obtains a Google signed ID Token to authenticate a Cloud Run request.
    /// </summary>
    /// <param name="credentialsFilePath">Path to the credentials .json file
    /// downloaded from https://console.cloud.google.com/apis/credentials.
    /// </param>
    /// <param name="uri">HTTP uri to fetch and get the token from.</param>
    /// <param name="cancellationToken">The token to propagate operation cancel notifications.</param>
    /// <returns>The http response message.</returns>
    public async Task<HttpResponseMessage> InvokeRequestAsync(
        string credentialsFilePath, string uri, CancellationToken cancellationToken = default)
    {
        // Read credentials from the credentials .json file.
        GoogleCredential credential = await GoogleCredential
            .FromFileAsync(credentialsFilePath, cancellationToken).ConfigureAwait(false);

        // Request an OIDC token for the Cloud Run secured resource.
        OidcToken oidcToken = await credential
            .GetOidcTokenAsync(OidcTokenOptions.FromTargetAudience(uri), cancellationToken).ConfigureAwait(false);

        // Always request the string token from the OIDC token,
        // the OIDC token will refresh the string token if it expires.
        string token = await oidcToken.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        // Include the OIDC token in an Authorization: Bearer header to 
        // the Cloud Run resource.
        using HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);
    }
}

