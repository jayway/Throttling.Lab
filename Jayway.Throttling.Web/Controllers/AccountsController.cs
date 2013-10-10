using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Jayway.Throttling.Web.Controllers
{
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
        public HttpResponseMessage Demo(string account)
        {
            bool allow = _throttlingService.Allow(account, 1, () => new Interval(60,10));
            return allow ? Request.CreateResponse(HttpStatusCode.OK) : Request.CreateResponse(HttpStatusCode.PaymentRequired, "THROTTLED");
        }

        [HttpPost("accounts/single/{account}/{cost}/{intervalInSeconds}/{creditsPerIntervalValue}")]
        public HttpResponseMessage Single(string account, int cost, int intervalInSeconds, long creditsPerIntervalValue)
        {
            bool allow = new ThrottledRequest(_throttlingService, cost, intervalInSeconds, creditsPerIntervalValue).Perform(account);
            return allow ? Request.CreateResponse(HttpStatusCode.OK) : Request.CreateResponse(HttpStatusCode.PaymentRequired, "THROTTLED");
        }

        [HttpPost("accounts/multi/{account}/{calls}/{accounts}/{cost}/{intervalInSeconds}/{creditsPerIntervalValue}")]
        public dynamic Multi(string account, int calls, int accounts, int cost, int intervalInSeconds, long creditsPerIntervalValue)
        {
            var r = new ThrottledRequest(_throttlingService, cost, intervalInSeconds, creditsPerIntervalValue);
            var throttledCount = 0;
            var s = Stopwatch.StartNew();
            for (var indx = 0; indx < calls; indx++)
            {
                var randomAccount = "a" + new Random().Next(accounts);
                if (!r.Perform(randomAccount))
                {
                    throttledCount++;
                }
            }
            var time = s.Elapsed;
            return new {calls, time, throttledCount};
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

        public bool Perform(String account) {
            return _throttlingService.Allow(account, _cost, _newInterval);
        }
    }

}
