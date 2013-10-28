using System.Configuration;
using Microsoft.ApplicationServer.Caching;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Jayway.Throttling
{
    public class AzureCacheThrottlingContext : IThrottlingContext
    {
        public IThrottlingService GetThrottlingService()
        {
            var account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            var client = account.CreateCloudTableClient();
            
            var table = client.GetTableReference("blaj");
            table.CreateIfNotExists();

            return new AzureCacheThrottlingService(table);
        }

        public void Close()
        {}
    }
}