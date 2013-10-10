using System.Configuration;
using Microsoft.ApplicationServer.Caching;

namespace Jayway.Throttling
{
    public class AzureCacheThrottlingContext : IThrottlingContext
    {
        public IThrottlingService GetThrottlingService()
        {
            var cache = new DataCache();
            //DataCacheFactoryConfiguration config = new DataCacheFactoryConfiguration
            //    {
                    
            //    }
            //var defaultCache = new DataCacheFactory(config).GetDefaultCache();

            return new AzureCacheThrottlingService(cache);
        }

        public void Close()
        {}
    }
}