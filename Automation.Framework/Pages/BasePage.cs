using OpenQA.Selenium;
using System;
using System.Linq;

namespace Automation.Framework.Pages
{
    public class BasePage
    {
        protected readonly IWebDriver _driver;
        public const int TIMEOUT_IN_SECONDS = 15;

        public BasePage(IWebDriver driver)
        {
            _driver = driver;
        }

        public static BasePage GetPage(string pageName, IWebDriver driver)
        {
            var pagesTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BasePage))));

            foreach (var t in pagesTypes)
            {
                var attr = t.GetCustomAttributes(typeof(PageObjectNameAttribute), false)
                    .OfType<PageObjectNameAttribute>()
                    .SingleOrDefault();

                if (attr == null) continue;
                if (attr.PageName.Equals(pageName, StringComparison.OrdinalIgnoreCase))
                {
                    return Activator.CreateInstance(t, driver) as BasePage;
                }
            }

            throw new PageObjectNotFoundException(pageName);
        }

        public T GetComponent<T>(string propertyName)
        {
            var t = this.GetType();
            var pageNameAttr = t.GetCustomAttributes(typeof(PageObjectNameAttribute), false)
                    .OfType<PageObjectNameAttribute>()
                    .SingleOrDefault();

            if (pageNameAttr == null) throw new PageObjectNameNotSetException(t.ToString());

            var propAndAttr = t.GetProperties()
                .Select(p => (property: p, attribute: p.GetCustomAttributes(typeof(PagePropertyNameAttribute), false).OfType<PagePropertyNameAttribute>().SingleOrDefault()))
                .Where(pa => pa.attribute != null)
                .Where(pa => pa.attribute.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                .SingleOrDefault();

            if (propAndAttr != default)
            {
                return (T)propAndAttr.property.GetValue(this);
            }

            throw new PagePropertyNotFoundException(t.ToString(), pageNameAttr.PageName, propertyName);
        }

        public IWebElement GetElement(string propertyName)
        {
            return GetComponent<IWebElement>(propertyName);
        }
    }

    public class PageObjectNameAttribute : Attribute
    {
        public PageObjectNameAttribute(string pageName)
        {
            PageName = pageName;
        }

        public string PageName { get; }
    }

    public class PagePropertyNameAttribute : Attribute
    {
        public PagePropertyNameAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }
    }

    public class PageObjectNotFoundException : Exception
    {
        public PageObjectNotFoundException(string pageName) : base($"Page object with name {pageName} was not finded") { }
    }

    public class PagePropertyNotFoundException : Exception
    {
        public PagePropertyNotFoundException(string pageType, string pageName, string propertyName) : base($"Page element '{propertyName}' was not finded in page '{pageName}' with type '{pageType}'") { }
    }

    public class PageObjectNameNotSetException : Exception
    {
        public PageObjectNameNotSetException(string pageType) : base($"Page Object with type {pageType} hasn't name") { }
    }
}
