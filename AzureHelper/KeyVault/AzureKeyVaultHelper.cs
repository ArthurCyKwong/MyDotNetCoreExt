using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;


namespace AzureHelper.KeyVault;
public static class AzureKeyVaultHelper
{
    const int defaultReloadHr = 24;
    public static IHostBuilder AddKeyVaultToConfiguration(this IHostBuilder builder)
    {
        return builder;
    }
    public static IConfigurationBuilder AddKeyVaultToConfiguration(this IConfigurationBuilder configBuilder,ILogger logger = null)
    {
        var ConfigManager = configBuilder.Build();
        var azureKeyVaultConfig = ConfigManager.GetSection("AzureKeyVault").Get<AzureKeyVaultConfig>();
        if (azureKeyVaultConfig is null)
        {           
            logger.LogError("Missing AzureKeyValue config Section");
            return configBuilder;
        }
        
        if (!azureKeyVaultConfig.ShouldEnable)
        {
            logger.LogError("Azure Key Vault disabled");
            return configBuilder;
        }        
        
        StoreLocation storeLocation = (azureKeyVaultConfig.StoreLocation)switch{
            "Local" => StoreLocation.LocalMachine,
            _=> StoreLocation.CurrentUser
        };
        using (var store = new X509Store(storeLocation))
        {
            store.Open(OpenFlags.ReadOnly);
            var certs = store.Certificates.Find(
                X509FindType.FindByThumbprint,
                azureKeyVaultConfig.AzureADCertThumbprint, false);

            var cert = certs.OfType<X509Certificate2>().SingleOrDefault();
            if (cert == null)
            {
                logger.LogError("Cert Not Found");
                return configBuilder;
            }
            logger.LogInformation("Cert subject {0}",cert.Thumbprint);

            configBuilder.AddAzureKeyVault(
                new Uri(azureKeyVaultConfig.KeyVaultUrl ??$"https://{azureKeyVaultConfig.KeyVaultName}.vault.azure.net/"),
                new ClientCertificateCredential( azureKeyVaultConfig.AzureADDirectoryId,
                        azureKeyVaultConfig.AzureADApplicationId,
                        certs.OfType<X509Certificate2>().Single()),
                new AzureKeyVaultConfigurationOptions(){
                    ReloadInterval = new TimeSpan(azureKeyVaultConfig.ReloadInterval?? defaultReloadHr,0,0)
                }
            );

            store.Close();
        }
        return configBuilder;
    }

    private static void LogError(this ILogger logger, string ErrorMsg, params object[] args){
        if (logger is null)
            Console.WriteLine(ErrorMsg,args);
        else 
            logger.LogError(ErrorMsg,args);
    }

    private static void LogInformation(this ILogger logger, string InfoMsg, params object[] args){
        if (logger is null)
            Console.WriteLine(InfoMsg,args);
        else 
            logger.LogInformation(InfoMsg,args);
    }

}
