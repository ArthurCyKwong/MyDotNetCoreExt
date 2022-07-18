

namespace AzureHelper.KeyVault;

public class AzureKeyVaultConfig{
    public string KeyVaultName{get;set;}
    public string KeyVaultUrl{get;set;}
    public string AzureADDirectoryId{get;set;}
    public string AzureADApplicationId{get;set;}
    public string AzureADCertThumbprint{get;set;}
    public bool ShouldEnable{get;set;}
    public string StoreLocation{get;set;}
    public int? ReloadInterval{get;set;}
}