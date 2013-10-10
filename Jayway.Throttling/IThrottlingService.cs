using System;
using System.Threading.Tasks;

namespace Jayway.Throttling
{
    public interface IThrottlingService
    {
        /// <summary>
        /// Decrease credits and check if the account is throttled.
        /// </summary>
        /// <param name="account">the unique id of the customer account</param>
        /// <param name="cost">the cost of this particular call</param>
        /// <param name="intervalFactory">callback to get the initial settings for an interval. In a
        /// real application it might take some time to get this value and
        /// therefore we should not do this on every request.</param>
        /// <returns>true if the request should be allowed, false if the account
        /// should be throttled</returns>
        Task<bool> Allow(string account, long cost, Func<Interval> intervalFactory);
    }
}