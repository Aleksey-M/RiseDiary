using System;
using System.Collections.Generic;
using System.Text;
using TechTalk.SpecFlow;

namespace Automation.WebUITests.Hooks
{
    [Binding]
    public class DefaultHooks
    {
        [BeforeScenario(Order = 0)]
        public void CreateWebDriver(ScenarioContext scenarioContext)
        {

        }

        [AfterScenario(Order = 50)]
        public void CloseWebDriver(ScenarioContext scenarioContext)
        {

        }

        [AfterTestRun]
        public static void CleanUp()
        {

        }
    }
}
