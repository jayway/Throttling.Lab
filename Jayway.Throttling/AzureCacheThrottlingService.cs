using System;
using System.Threading.Tasks;
using Microsoft.ApplicationServer.Caching;
using Microsoft.WindowsAzure.Storage.Table;

namespace Jayway.Throttling
{
    public class AzureCacheThrottlingService : IThrottlingService
    {
        private readonly CloudTable _table;

        public AzureCacheThrottlingService(CloudTable table)
        {
            _table = table;
        }

        public async Task<bool> Allow(string account, long cost, Func<Interval> intervalFactory)
        {
            var interval = intervalFactory();
            var result = await _table.DecrementWithTimeout(account, cost,interval.Credits, TimeSpan.FromSeconds(interval.Seconds));

            return result > 0;
        }
    }
}