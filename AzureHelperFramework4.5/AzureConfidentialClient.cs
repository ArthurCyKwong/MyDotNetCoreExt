using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureHelperFramework
{
    public class AzureConfidentialClient
    {
        private IConfidentialClientApplication _app;
        public IConfidentialClientApplication app
        {
            get
            {
                if (_app is null)
                {
                    _app = ConfidentialClientApplicationBuilder.Create(_ClientId)
                        .WithClientSecret(_Secret)
                        .WithAuthority(new Uri($"{_Instance}{_Tenant}"))
                        .WithCacheOptions(new CacheOptions()
                        {
                            UseSharedCache = false
                        })
                        .Build();
                }
                return _app;
            }
        }
        public readonly string _Instance;
        public readonly string _Tenant;
        public readonly string _ClientId;
        public readonly string _Secret;
        public AzureConfidentialClient(string Instance, string Tenant, string ClientId, string Secret)
        {
            _Instance = Instance;
            _Tenant = Tenant;
            _ClientId = ClientId;
            _Secret = Secret;
        }




        public string GetAccessToken(IConfidentialClientApplication app, string[] scope)
        {
            try
            {
                var Task = app.AcquireTokenForClient(scope).ExecuteAsync();
                Task.Wait();
                var Result = Task.Result;


                return Result.AccessToken;

            }
            catch (Exception ex)
            {
                var Err = ex.ToString();
                return null;
            }
        }



    }
}
