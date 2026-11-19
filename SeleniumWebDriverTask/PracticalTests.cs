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
        private IWebDriver driver;
        private WebDriverWait wait;
        private IJavaScriptExecutor js;

        [SetUp]
        public void SetUp()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            baseUrl = _configuration["Settings:BaseUrl"];

            Assert.IsFalse(string.IsNullOrEmpty(baseUrl), "BaseUrl should not be empty in appsettings.json.");

            driver = new ChromeDriver();
            js = (IJavaScriptExecutor)driver;

            driver.Manage().Window.Maximize();

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }

        private void AcceptCookies()
        {
            var acceptCookieButton = wait.Until(d =>
            d.FindElement(By.Id("onetrust-accept-btn-handler")));

            Thread.Sleep(50);
            acceptCookieButton.Click();
            //js.ExecuteScript("arguments[0].click();", acceptCookieButton);
        }

        [Test]
        public void Navigation_To_Job_Search_Page_Test()
        {
            //Start page
            driver.Url = baseUrl;

            //Cookie button click
            AcceptCookies();

            //Go to careers page
            var careersLink = driver.FindElement(By.XPath("//ul[contains(@class, 'top-navigation__row')]//descendant::a[contains(@class, 'top-navigation__item-link') and text()='Careers']"));
            careersLink.Click();

            //Go to job search page
            var searchButton = driver.FindElement(By.CssSelector("div.pinned-button div[data-gtm-category='job_search_redirect']"));
            searchButton.Click();

            //Accept cookie
            AcceptCookies();

            // Verify we successfully landed on the job search page
            var inputKeyword = wait.Until(d => driver.FindElement(By.XPath("//input[@data-testid='search-input']")));
            Assert.IsTrue(inputKeyword.Displayed, "Job search input should be visible after navigation.");
        }

        [TestCase("Ukraine", "C#")]
        public void Job_Search_Shows_Results_Test(string location, string keyword)
        {
            //Start page
            driver.Url = baseUrl;

            //Cookie button click
            AcceptCookies();

            //Go to careers page
            var careersLink = driver.FindElement(By.XPath("//ul[contains(@class, 'top-navigation__row')]//descendant::a[contains(@class, 'top-navigation__item-link') and text()='Careers']"));
            careersLink.Click();

            //Go to job search page
            var searchButton = driver.FindElement(By.CssSelector("div.pinned-button div[data-gtm-category='job_search_redirect']"));
            searchButton.Click();

            //Accept cookie
            AcceptCookies();

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

            //Find last job card to verify results are displayed
            var lastJob = wait.Until(d => driver.FindElement(By.CssSelector("div[class^='JobCard_panel_']:last-child a[data-testid='job-card-link']")));
            Assert.IsTrue(lastJob.Displayed, "Filtered job cards should be displayed.");
        }

        [TestCase("All", "Automation")]
        public void All_Steps_Test(string location, string keyword)
        {

            //Start page
            driver.Url = baseUrl;

            //Cookie button click
            AcceptCookies();

            //Go to careers page
            var careersLink = driver.FindElement(By.XPath("//ul[contains(@class, 'top-navigation__row')]//descendant::a[contains(@class, 'top-navigation__item-link') and text()='Careers']"));
            careersLink.Click();

            //Go to job search page
            var searchButton = driver.FindElement(By.CssSelector("div.pinned-button div[data-gtm-category='job_search_redirect']"));
            searchButton.Click();

            //Accept cookie
            AcceptCookies();

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
        }
    }
}
