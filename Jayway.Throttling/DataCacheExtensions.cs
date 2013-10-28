using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.ApplicationServer.Caching;
using Microsoft.WindowsAzure.Storage.Table;

namespace Jayway.Throttling
{
    public class BlajEntity : TableEntity
    {
        public long Credit { get; set; }
        public long Calls { get; set; }
        public DateTime Expires { get; set; }
    }

    public static class DataCacheExtensions
    {
        public static TimeSpan Get;
        public static TimeSpan Add;
        public static TimeSpan Put;

        public static async Task<long> DecrementWithTimeout(this CloudTable table, string key, long amount, long initialValue,
                                                TimeSpan timeOut)
        {
                var get = Stopwatch.StartNew();
            TableResult item;
                     item = await table.ExecuteAsync(TableOperation.Retrieve<BlajEntity>("partion", key));
            get.Stop();
                Get = Get.Add(get.Elapsed);
                var s = Stopwatch.StartNew();
                if (item.Result == null)
                {
                    var add = Stopwatch.StartNew();
                    var te = new BlajEntity();
                    te.PartitionKey = "partion";
                    te.RowKey = key;
                    te.Credit = initialValue;
                    te.Expires = DateTime.Now.Add(timeOut);
                    await table.ExecuteAsync(TableOperation.InsertOrReplace(te));
                    add.Stop();
                    Add = Add.Add(add.Elapsed);
                    Debug.WriteLine("\"{0}\" expires in {1}", key, timeOut);
                    return initialValue - amount;
                }
                else
                {
                    var blaj = item.Result as BlajEntity;
                    if (blaj.Expires > DateTime.Now) return 0;

                    if (blaj.Calls++ >= blaj.Credit) return 0;

                    //Debug.WriteLine("\"{0}\" expires in {1}", key, item.Timeout - s.Elapsed);
                    var put = Stopwatch.StartNew();

                    await table.ExecuteAsync(TableOperation.Replace(blaj));

                    put.Stop();
                    Put = Put.Add(put.Elapsed);
                    return blaj.Calls;
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