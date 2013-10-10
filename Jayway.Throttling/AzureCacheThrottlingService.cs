using System;
using System.Threading.Tasks;
using Microsoft.ApplicationServer.Caching;

namespace Jayway.Throttling
{
    public class AzureCacheThrottlingService : IThrottlingService
    {
        private readonly DataCache _dataCache;

        public AzureCacheThrottlingService(DataCache dataCache)
        {
            _dataCache = dataCache;
        }

        public async Task<bool> Allow(string account, long cost, Func<Interval> intervalFactory)
        {
            var interval = intervalFactory();
            var result = await _dataCache.DecrementWithTimeout(account, cost,interval.Credits, TimeSpan.FromSeconds(interval.Seconds));

            return result > 0;
        }
    }
}