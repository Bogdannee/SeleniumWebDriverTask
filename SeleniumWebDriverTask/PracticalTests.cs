using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
// to avoid conflict with Microsoft.Testing.Platform.Configurations.IConfigurationRoot
using IConfigRoot = Microsoft.Extensions.Configuration.IConfigurationRoot;

namespace SeleniumWebDriverTask
{
    [TestFixture]
    public class PracticalTests
    {
        private IConfigRoot _configuration;
        private string baseUrl;

        [SetUp]
        public void SetUp()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            baseUrl = _configuration["Settings:BaseUrl"];

            Assert.IsFalse(string.IsNullOrEmpty(baseUrl), "BaseUrl should not be empty in appsettings.json.");
        }

        [Test]
        public void TestNavigationWithJsonConfig()
        {
            IWebDriver driver = new ChromeDriver();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            driver.Manage().Window.Maximize();
            driver.Url = @"https://www.epam.com/";

            // 1.Start page
            var acceptCookieButton = wait.Until(d => {
                try
                {
                    var element = d.FindElement(By.Id("onetrust-accept-btn-handler"));
                    return (element.Displayed && element.Enabled) ? element : null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
            });

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].click();", acceptCookieButton);

            var careersLink = driver.FindElement(By.XPath("//ul[contains(@class, 'top-navigation__row')]//descendant::a[contains(@class, 'top-navigation__item-link') and text()='Careers']"));
            careersLink.Click();

            // 2.Careers page
            var searchButton = driver.FindElement(By.CssSelector("div.pinned-button div[data-gtm-category='job_search_redirect']"));
            searchButton.Click();

            // 3.Job search page
            var acceptCookieButton1 = wait.Until(d =>
                d.FindElement(By.Id("onetrust-accept-btn-handler")));
            acceptCookieButton1.Click();

            acceptCookieButton1.Click();

            //Choose location
            var locationDropdown = wait.Until(d => d.FindElement(By.CssSelector("div[data-testid='country-dropdown'] input.dropdown__input")));
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", locationDropdown);

            locationDropdown.SendKeys("Ukraine");

            var locationOptionToSelect = wait.Until(d => d.FindElement(By.XPath("//div[@role='listbox']//div[contains(., 'Ukraine')]")));
            locationOptionToSelect.Click();

            //Choose Workplace type remote
            var optionRemote = driver.FindElement(By.XPath("//div[not(@data-testid = 'mobile-filter-item')]/fieldset[@aria-labelledby='Workplace type-filter-title']//label[contains(@for, 'checkbox-vacancy_type-Remote')]"));
            js.ExecuteScript("arguments[0].click();", optionRemote);

            //Enter search text
            var inputKeyword = wait.Until(d => driver.FindElement(By.XPath("//input[@data-testid='search-input']")));
            inputKeyword.SendKeys("C#");

            var searchJobsButton = wait.Until(d => driver.FindElement(By.Name("submit_search_box_button")));
            searchJobsButton.Click();

            //Search results
            var lastJob = wait.Until(d => driver.FindElement(By.CssSelector("div[class^='JobCard_panel_']:last-child a[data-testid='job-card-link']")));

            var actions = new Actions(driver);
            actions.MoveToElement(lastJob)
                .Click()
                .Perform();

            var searchQueryExist = driver.FindElement(By.XPath("//main[contains(., 'C#')]"));

            //Assert.IsNotEmpty(searchQueryExist);
            driver.Quit();
        }
    }
}
