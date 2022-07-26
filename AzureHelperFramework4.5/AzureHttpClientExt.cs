using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureHelperFramework
{
    public static class AzureHttpClientExt
    {        
        public static HttpClient PrepareHttpClient(this AzureConfidentialClient client, string[] Scope)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var AccessToken = client.GetAccessToken(client.app, Scope);

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            httpClient.DefaultRequestHeaders.Add("Accept", "*/*");

            return httpClient;
        }
    }
}
