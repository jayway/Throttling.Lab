using System;
using System.Collections.Generic;
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

        public AccountsController() : this(new AllowAllThrottlingContext() )
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

        [HttpPost("single/{account}")]
        public HttpResponseMessage Single(string account, int cost, int intervalInSeconds, long creditsPerIntervalValue)
        {
            bool allow = new ThrottledRequest(_throttlingService, cost, intervalInSeconds, creditsPerIntervalValue).Perform(account);
            return allow ? Request.CreateResponse(HttpStatusCode.OK) : Request.CreateResponse(HttpStatusCode.PaymentRequired, "THROTTLED");
        }

        [HttpPost("multi")]
        public dynamic Multi(TestModel model)
        {
			var r = new ThrottledRequest(_throttlingService, model.Cost, model.IntervalInSeconds, model.CreditsPerIntervalValue);
            var throttledCount = 0;
            long startTime = DateTime.Now.Millisecond; //System.currentTimeMillis();
            for (int i = 0; i < model.Accounts; i++)
            {
                var randomAccount = "a" + new Random().Next() * model.Accounts;
                for (var indx = 0; indx < model.Calls; indx++)
                {
                    if (!r.Perform(randomAccount))
                    {
                        throttledCount++;
                    }
                }
                
            }
            var time = DateTime.Now.Millisecond - startTime;
            return new {model.Calls, time, throttledCount};
        }

		public class TestModel
		{
			public int Calls { get; set; }
			public int Accounts { get; set; }
			public int Cost { get; set; }
			public int IntervalInSeconds { get; set; }
			public long CreditsPerIntervalValue { get; set; }
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
