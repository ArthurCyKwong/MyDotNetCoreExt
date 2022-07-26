using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureHelperFramework;
using System.Net.Http;
using Microsoft.Exchange.WebServices.Data;

namespace AzureHelperFramework.Sample
{
    internal class SampleClass
    {
        private string Instance = ConfigurationManager.AppSettings["AzureAd:Instance"];
        private string Tenant = ConfigurationManager.AppSettings["AzureAd:TenantId"];
        private string ClientId = ConfigurationManager.AppSettings["AzureAd:ClientId"];
        private string Secret = ConfigurationManager.AppSettings["AzureAd:ClientSecret"];


        public void AzureClientHttpCall()
        {
            var AzureClient = new AzureConfidentialClient(Instance, Tenant, ClientId, Secret);
            string[] Scope = ConfigurationManager.AppSettings["HttpCall:Scope"].Split(' ').ToArray();
            var client = AzureClient.PrepareHttpClient(Scope);

            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://Google.com"),
                Method = HttpMethod.Post,
                Content = new StringContent("Hello World", Encoding.UTF8, "application/json"),
            };

            var ResponseTask = client.SendAsync(requestMessage);
            ResponseTask.Wait();
            var Response = ResponseTask.Result;                      
        }

        private string strExchangeUri { get { return ConfigurationManager.AppSettings["EWS.URI"]; } }

        public void AzureExchangeSendEmail(string EmailFrom, IEnumerable<string> EmailTo, string Subject, string MailBody, IEnumerable<string> EmailCC = null, IEnumerable<string> EmailBCC = null)
        {
            var AzureClient = new AzureConfidentialClient(Instance, Tenant, ClientId, Secret);
            string[] Scope = ConfigurationManager.AppSettings["ExchangeService:Scope"].Split(' ').ToArray();

            var exchangeService = AzureClient.GetAzureExchangeService(strExchangeUri,ConnectingIdType.SmtpAddress, EmailFrom, Scope);
            var email = new EmailMessage(exchangeService);
            email.ToRecipients.AddRange(EmailTo);
            email.Subject = Subject;
            email.Body = new MessageBody(BodyType.HTML, MailBody);

            if (!(EmailCC is null || EmailCC.Count() <= 0)) email.CcRecipients.AddRange(EmailCC);
            if (!(EmailBCC is null || EmailBCC.Count() <= 0)) email.BccRecipients.AddRange(EmailBCC);
            email.SendAndSaveCopy();
        }

        public FindItemsResults<Item> AzureExcnahgeReadEmail()
        {
            var AzureClient = new AzureConfidentialClient(Instance, Tenant, ClientId, Secret);
            string[] Scope = ConfigurationManager.AppSettings["ExchangeService:Scope"].Split(' ').ToArray();

            var exchangeService = AzureClient.GetAzureExchangeService(strExchangeUri, ConnectingIdType.SmtpAddress, "<AzureAd>@gammonconstruction.com", Scope);

            SearchFilter searchFilterLastDays = new SearchFilter.IsGreaterThanOrEqualTo(ItemSchema.DateTimeReceived, DateTime.Now.Date.AddDays(-10));
            SearchFilter searchFilterSubjectSystem = new SearchFilter.ContainsSubstring(ItemSchema.Subject, "System:");
            SearchFilter searchFilterSubjectApprovalId = new SearchFilter.ContainsSubstring(ItemSchema.Subject, "Approval Id:");

            SearchFilter searchFilterAnd = new SearchFilter.SearchFilterCollection(LogicalOperator.And, searchFilterLastDays, searchFilterSubjectSystem, searchFilterSubjectApprovalId);

            ItemView itemView = new ItemView(int.MaxValue);

            FindItemsResults<Item> findItemResults = exchangeService.FindItems(WellKnownFolderName.Inbox, searchFilterAnd, itemView);

            if (findItemResults.TotalCount <= 0)
                return null;

            exchangeService.LoadPropertiesForItems(findItemResults, PropertySet.FirstClassProperties);
            return findItemResults;
        }
    }

}
