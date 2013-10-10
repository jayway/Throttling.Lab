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

        public bool Allow(string account, long cost, Func<Interval> getInitialValues)
        {
            var initialValues = getInitialValues();
            //var result = _dataCache.Decrement(account, cost, interval.Credits - cost);

            long newValue = 0;
            DataCacheItem item = _dataCache.GetCacheItem(account);
            TimeSpan timeRemaining;
            if (item == null)
            {
                newValue = initialValues.Credits - cost;
                timeRemaining = TimeSpan.FromSeconds(initialValues.Seconds);
                _dataCache.Put(account, newValue, timeRemaining);
            }
            else
            {
                // Decrement counter
                var oldValue = ((long)item.Value);
                newValue = oldValue - cost;

                if (newValue > 0)
                {
                    _dataCache.Put(account, newValue, item.Timeout);
                }

                timeRemaining = item.Timeout;
            }

            Debug.WriteLine("Account: {0} Value: {2} TTL: {1}", account, timeRemaining, newValue);

            return newValue > 0;
        }
    }
}