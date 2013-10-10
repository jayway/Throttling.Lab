namespace Jayway.Throttling
{

    public interface IThrottlingContext
    {
        IThrottlingService GetThrottlingService();

        void Close();
    }

	public class AllowAllThrottlingContext : IThrottlingContext
	{
		public IThrottlingService GetThrottlingService()
		{
			return new AllowAllThrottlingService();
		}

		public void Close()
		{
			
		}
	}
}