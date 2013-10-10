using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Jayway.Throttling.Web.Controllers
{
    public class TestModel
    {
        public int Calls { get; set; }
        public int Accounts { get; set; }
        public int Cost { get; set; }
        public int IntervalInSeconds { get; set; }
        public long CreditsPerIntervalValue { get; set; }
    }

    public class AccountsController : ApiController
    {
        private readonly IThrottlingService _throttlingService;

        public AccountsController() : this(new AzureCacheThrottlingContext())
        {}

        public AccountsController(IThrottlingContext throttlingContext)
        {
            _throttlingService = throttlingContext.GetThrottlingService();
        }
        
        [HttpPost("accounts/{account}/demo")]
        public async Task<HttpResponseMessage> Demo(string account)
        {
            DataCacheExtensions.Clear();
            bool allow = await _throttlingService.Allow(account, 1, () => new Interval(60,10));
            return allow ? Request.CreateResponse(HttpStatusCode.OK) : Request.CreateResponse(HttpStatusCode.PaymentRequired, "THROTTLED");
        }

        [HttpPost("single/{account}")]
        public async Task<HttpResponseMessage> Single(string account, TestModel model)
        {
            DataCacheExtensions.Clear();
            bool allow = await new ThrottledRequest(_throttlingService, model.Cost, model.IntervalInSeconds, model.CreditsPerIntervalValue).Perform(account);
            return allow ? Request.CreateResponse(HttpStatusCode.OK) : Request.CreateResponse(HttpStatusCode.PaymentRequired, "THROTTLED");
        }

        [HttpPost("multi/{account}")]
        public async Task<dynamic> Multi(string account, TestModel model)
        {
            DataCacheExtensions.Clear();

            var r = new ThrottledRequest(_throttlingService, model.Cost, model.IntervalInSeconds, model.CreditsPerIntervalValue);
            var throttledCount = 0;
            var sw = Stopwatch.StartNew();
            for (var indx = 0; indx < model.Calls; indx++)
            {
                var randomAccount = "a" + new Random().Next(model.Accounts);
                if (!await r.Perform(randomAccount))
                {
                    throttledCount++;
                }
            }
            sw.Stop();

            Debug.WriteLine("Get: {0}", DataCacheExtensions.Get);
            Debug.WriteLine("Add: {0}", DataCacheExtensions.Add);
            Debug.WriteLine("Put: {0}", DataCacheExtensions.Put);

            return new {model.Calls, sw.ElapsedMilliseconds, throttledCount, DataCacheExtensions.Get, DataCacheExtensions.Add, DataCacheExtensions.Put};
        }
    }


    //TODO create filter
    public class ThrottledRequest {
        private readonly long _cost;
        private readonly Func<Interval> _newInterval;
        private readonly IThrottlingService _throttlingService;


        public ThrottledRequest(IThrottlingService throttlingService, int cost, int intervalInSeconds, long creditsPerIntervalValue) {
            this._throttlingService = throttlingService;
            this._cost = cost;
            this._newInterval = () => new Interval(intervalInSeconds, creditsPerIntervalValue);
        }

        public async Task<bool> Perform(String account) {
            return await _throttlingService.Allow(account, _cost, _newInterval);
        }
    }

}
