namespace Supa.Tests.TestDoubles
{
    using Supa.Platform;

    public class TestableExchangeSource : ExchangeSource
    {
        public TestableExchangeSource(string folderName, IExchangeServiceProvider exchangeServiceProvider)
            : base(folderName, exchangeServiceProvider)
        {
        }
    }
}