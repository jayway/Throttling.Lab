using System;
using System.Diagnostics;
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

        public bool Allow(string account, long cost, Func<Interval> intervalFactory)
        {
            var interval = intervalFactory();

            long result;

            var cacheItem = _dataCache.GetCacheItem(account);
            if (cacheItem == null)
            {
                result = interval.Credits - cost;
                _dataCache.Put(account, result, TimeSpan.FromSeconds(interval.Seconds));
            }
            else
            {
                result = (long)cacheItem.Value - cost;
                _dataCache.Put(account, result, cacheItem.Timeout);
            }
            
            return result > 0;
        }
    }
}