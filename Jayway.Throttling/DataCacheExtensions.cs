using Microsoft.ApplicationServer.Caching;
using System;
using System.Diagnostics;

namespace Jayway.Throttling
{
    public static class DataCacheExtensions
    {
        public static TimeSpan Get;
        public static TimeSpan Add;
        public static TimeSpan Put;

        public static long Decrement(this DataCache dataCache, string key, long amount, long initialValue,
                                                TimeSpan timeOut)
        {
            var get = Stopwatch.StartNew();
            var item = dataCache.GetCacheItem(key);
            get.Stop();
            Get = Get.Add(get.Elapsed);
            //var s = Stopwatch.StartNew();
            if (item == null)
            {
                var add = Stopwatch.StartNew();
                dataCache.Put(key, initialValue - amount, timeOut);
                add.Stop();
                Add = Add.Add(add.Elapsed);
                //Debug.WriteLine("\"{0}\" expires in {1}", key, timeOut);
                return initialValue - amount;
            }
            else
            {
                var value = (long)item.Value;
                if (value <= 0) return 0;

                //Debug.WriteLine("\"{0}\" expires in {1}", key, item.Timeout - s.Elapsed);
                var put = Stopwatch.StartNew();
                var result = dataCache.Decrement(key, value, initialValue);
                dataCache.ResetObjectTimeout(key, item.Timeout);
                var newValue = Math.Max(result, 0);
                put.Stop();
                Put = Put.Add(put.Elapsed);
                return newValue;
            }

        }

        public static void Clear()
        {
            Get = TimeSpan.Zero;
            Add = TimeSpan.Zero;
            Put = TimeSpan.Zero;
        }
    }
}