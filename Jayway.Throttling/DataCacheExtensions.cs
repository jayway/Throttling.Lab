using System;
using System.Diagnostics;
using Microsoft.ApplicationServer.Caching;

namespace Jayway.Throttling
{
    public static class DataCacheExtensions
    {
        public static long DecrementWithTimeout(this DataCache dataCache, string key, long amount, long initialValue,
                                                TimeSpan timeOut)
        {
            var item = dataCache.GetCacheItem(key);
            var s = new Stopwatch();
            s.Start();
            if (item == null)
            {
                dataCache.Add(key, initialValue - amount, timeOut);
                Debug.WriteLine("\"{0}\" expires in {1}", key, timeOut);
                return initialValue - amount;
            }
            else
            {
                var value = (long)item.Value;
                if (value <= 0) return 0;
                var newValue = Math.Max(value - amount, 0);
                Debug.WriteLine("\"{0}\" expires in {1}", key, item.Timeout - s.Elapsed);
                dataCache.Put(key, newValue, item.Timeout - s.Elapsed);
                return newValue;
            }
        }
    }
}