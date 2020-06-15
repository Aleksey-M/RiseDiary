using Automation.Framework.Pages;
using OpenQA.Selenium;
using TechTalk.SpecFlow;

namespace Automation.WebUITests.CommonSteps
{
    public class BaseSteps
    {
        protected readonly IWebDriver _driver;
        protected readonly ScenarioContext _scenarioContext;

        public BaseSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _driver = _scenarioContext["WEB_DRIVER"] as IWebDriver;
        }

        public T GetCurrentPage<T>() where T : BasePage => (_scenarioContext.Get<BasePage>("CURRENT_PAGE") as T) ?? throw new System.Exception("Current page is not set on previous step");

        public void SetCurentPage(BasePage page) => _scenarioContext.Set(page, "CURRENT_PAGE");
    }
}
