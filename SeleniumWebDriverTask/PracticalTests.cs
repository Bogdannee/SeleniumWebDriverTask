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
        [TestCase("All", "Automation")]
        public void TestNavigationWithJsonConfig(string location, string keyword)
        {
            IWebDriver driver = new ChromeDriver();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            driver.Manage().Window.Maximize();

            //Start page
            driver.Url = @"https://www.epam.com/";

            //Cookie button click
            var acceptCookieButton = wait.Until(d =>
            d.FindElement(By.Id("onetrust-accept-btn-handler")));

            Thread.Sleep(50);
            acceptCookieButton.Click();

            //var acceptCookieButton = wait.Until(d =>
            //{
            //    try
            //    {
            //        var element = d.FindElement(By.Id("onetrust-accept-btn-handler"));
            //        return (element.Displayed && element.Enabled) ? element : null;
            //    }
            //    catch (NoSuchElementException)
            //    {
            //        return null;
            //    }
            //});
            //js.ExecuteScript("arguments[0].click();", acceptCookieButton);

            //Go to careers page
            var careersLink = driver.FindElement(By.XPath("//ul[contains(@class, 'top-navigation__row')]//descendant::a[contains(@class, 'top-navigation__item-link') and text()='Careers']"));
            careersLink.Click();

            //Go to job search page
            var searchButton = driver.FindElement(By.CssSelector("div.pinned-button div[data-gtm-category='job_search_redirect']"));
            searchButton.Click();

            //Accept cookie
            var acceptCookieButton1 = wait.Until(d =>
                d.FindElement(By.Id("onetrust-accept-btn-handler")));
            acceptCookieButton1.Click();

            //Choose location
            var locationDropdown = wait.Until(d => d.FindElement(By.CssSelector("div[data-testid='country-dropdown'] input.dropdown__input")));
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", locationDropdown);

            locationDropdown.SendKeys(location);

            var locationOptionToSelect = wait.Until(d => d.FindElement(By.XPath($"//div[@role='listbox']//div[contains(., '{location}')]")));
            locationOptionToSelect.Click();

            //Choose Workplace type remote
            var optionRemote = driver.FindElement(By.XPath("//div[not(@data-testid = 'mobile-filter-item')]/fieldset[@aria-labelledby='Workplace type-filter-title']//label[contains(@for, 'checkbox-vacancy_type-Remote')]"));
            optionRemote.Click();

            //Enter keywords in input
            var inputKeyword = wait.Until(d => driver.FindElement(By.XPath("//input[@data-testid='search-input']")));
            inputKeyword.SendKeys(keyword);

            //Search button click
            var searchJobsButton = wait.Until(d => driver.FindElement(By.Name("submit_search_box_button")));
            searchJobsButton.Click();

            //Find last job card
            var lastJob = wait.Until(d => driver.FindElement(By.CssSelector("div[class^='JobCard_panel_']:last-child a[data-testid='job-card-link']")));

            //Go to job card page
            var actions = new Actions(driver);
            actions.MoveToElement(lastJob)
                .Click()
                .Perform();

            //Try find keywords on page
            var mainElement = wait.Until(d =>
                d.FindElements(By.TagName("main")).FirstOrDefault(e => e.Displayed));

            Assert.That(mainElement.Text, Does.Contain(keyword).IgnoreCase,
                $"Keyword '{keyword}' was not found on the job page.");

            driver.Quit();
        }
    }
}
