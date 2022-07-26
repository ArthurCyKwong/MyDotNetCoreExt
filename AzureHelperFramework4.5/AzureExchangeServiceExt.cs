using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AzureHelperFramework
{
    public static class AzureExchangeServiceExt
    {
        public static ExchangeService GetAzureExchangeService(this AzureConfidentialClient client ,string ExchangeUrl, ConnectingIdType IdType, string EmailId, string[] Scope)
        {
            var AccessToken = client.GetAccessToken(client.app, Scope);
            var exchangeService = new ExchangeService();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | (SecurityProtocolType)3072;

            exchangeService.Credentials = new OAuthCredentials(AccessToken);
            exchangeService.ImpersonatedUserId = new ImpersonatedUserId(IdType, EmailId);
            exchangeService.Url = new Uri(ExchangeUrl);

            return exchangeService;
        }
    }
}
