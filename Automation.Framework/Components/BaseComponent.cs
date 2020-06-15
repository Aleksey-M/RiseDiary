using Automation.Framework.Extensions;
using OpenQA.Selenium;

namespace Automation.Framework.Components
{
    public class BaseComponent
    {
        protected readonly IWebDriver _driver;
        protected readonly By _by;
        protected readonly int _timeoutInSeconds;

        public BaseComponent(IWebDriver driver, By by, int timeoutInSeconds = 15)
        {
            _driver = driver;
            _by = by;
            _timeoutInSeconds = timeoutInSeconds;
        }

        public IWebElement Element => _driver.FindElement(_by, _timeoutInSeconds);

        public bool IsElementExists(int milliseconds) => _driver.IsElementExist(_by, milliseconds);
    }
}
