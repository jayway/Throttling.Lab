using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ApplicationServer.Caching;

namespace Jayway.Throttling
{
    public static class DataCacheExtensions
    {
        public static TimeSpan Get;
        public static TimeSpan Add;
        public static TimeSpan Put;

        public static async Task<long> DecrementWithTimeout(this DataCache dataCache, string key, long amount, long initialValue,
                                                TimeSpan timeOut)
        {
            return await Task.Run(() =>
            {
                var get = Stopwatch.StartNew();
                var item = dataCache.GetCacheItem(key);
                get.Stop();
                Get = Get.Add(get.Elapsed);
                var s = Stopwatch.StartNew();
                if (item == null)
                {
                    var add = Stopwatch.StartNew();
                    dataCache.Put(key, initialValue - amount, timeOut);
                    add.Stop();
                    Add = Add.Add(add.Elapsed);
                    Debug.WriteLine("\"{0}\" expires in {1}", key, timeOut);
                    return initialValue - amount;
                }
                else
                {
                    var value = (long)item.Value;
                    if (value <= 0) return 0;
                    var newValue = Math.Max(value - amount, 0);
                    Debug.WriteLine("\"{0}\" expires in {1}", key, item.Timeout - s.Elapsed);
                    var put = Stopwatch.StartNew();
                    dataCache.Decrement(key, newValue, initialValue);
                    dataCache.ResetObjectTimeout(key,item.Timeout);
                    put.Stop();
                    Put = Put.Add(put.Elapsed);
                    return newValue;
                }
            });
        }

        public static void Clear()
        {
            Get = TimeSpan.Zero;
            Add = TimeSpan.Zero;
            Put = TimeSpan.Zero;
        }
    }
}