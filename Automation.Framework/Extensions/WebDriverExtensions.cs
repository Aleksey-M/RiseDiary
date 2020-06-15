using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace Automation.Framework.Extensions
{
    public static class WebDriverExtensions
    {
        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
            return wait.Until(drv => drv.FindElement(by));
        }

        public static bool IsElementExist(this IWebDriver driver, By by, int waitInMilliseconds)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(waitInMilliseconds));
                wait.Until(drv => drv.FindElement(by));
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }
    }
}
