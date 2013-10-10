namespace Jayway.Throttling.Web.Models
{
    public class SingleModel
    {
        public string Account { get; set; }
        public int Cost { get; set; }
        public long IntervalInSeconds { get; set; }
        public long CreditsPerIntervalValue { get; set; }
    }
}