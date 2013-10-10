using System.Configuration;
using Microsoft.ApplicationServer.Caching;
using Microsoft.WindowsAzure.Storage;

namespace Jayway.Throttling
{
    public class AzureCacheThrottlingContext : IThrottlingContext
    {
        public IThrottlingService GetThrottlingService()
        {
            var account = CloudStorageAccount.Parse("DefaultEndpointsProtocol=http;AccountName=jakobnekdag;AccountKey=1DZD2wHiDfulTee9R4l+cpnavuOuUe/6MJ1Yi022baQBnyQfJQnjM5VWHw1mQHgY6n4g+R7Tq21rSeiJwylGKA==");
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference("blaj");
            table.CreateIfNotExists();

            return new AzureCacheThrottlingService(table);
        }

        public void Close()
        {}
    }
}