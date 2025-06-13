using Avalonia;
using NUnit.Framework;
using Vosmerka;

namespace TestsProduct
{
    public abstract class TestBase
    {
        protected static AppBuilder _appBuilder;
        protected static bool _isInitialized;

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            if (!_isInitialized)
            {
                _appBuilder = AppBuilder.Configure<App>()
                    .UsePlatformDetect()
                    .SetupWithoutStarting();
                _isInitialized = true;
            }
        }
    }
}