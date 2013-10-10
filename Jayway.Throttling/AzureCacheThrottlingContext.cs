using System.Configuration;
using Microsoft.ApplicationServer.Caching;

namespace Jayway.Throttling
{
    public class AzureCacheThrottlingContext : IThrottlingContext
    {
        public IThrottlingService GetThrottlingService()
        {
            //throw new ConfigurationErrorsException("Configure Cache Service!");
            var cache = new DataCache(); 
            return new AzureCacheThrottlingService(cache);
        }

        public void Close()
        {}
    }
}