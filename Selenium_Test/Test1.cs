
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Selenium_Test
{
    [TestClass]
    public class UsingSeleniumTest
    {
        private static IWebDriver driver;
        private static WebDriverWait wait;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--window-size=1920,1080");

            driver = new ChromeDriver(options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            driver?.Quit();
        }

        [TestInitialize]
        public void OpenPage()
        {
            driver.Navigate().GoToUrl("http://127.0.0.1:5500");

            wait.Until(d =>
                d.FindElements(By.ClassName("navbar-brand")).Count > 0);
        }

        // Helper methods

        private void Login()
        {
            driver.FindElement(By.CssSelector(
                "input[placeholder='Username']")).SendKeys("admin");

            driver.FindElement(By.CssSelector(
                "input[placeholder='Password']")).SendKeys("1234");

            driver.FindElement(By.XPath(
                "//button[normalize-space()='Login']")).Click();

            wait.Until(d =>
                d.FindElements(By.XPath(
                    "//button[normalize-space()='Logout']")).Count > 0);
        }

        private void AddRecord(
            string title,
            string artist,
            string duration,
            string year)
        {
            wait.Until(d =>
                d.FindElements(By.Id("record-title")).Count > 0);

            driver.FindElement(By.Id("record-title"))
                .SendKeys(title);

            driver.FindElement(By.Id("record-artist"))
                .SendKeys(artist);

            driver.FindElement(By.Id("record-duration"))
                .SendKeys(duration);

            driver.FindElement(By.Id("record-publication-year"))
                .SendKeys(year);

            driver.FindElement(By.XPath(
                "//button[normalize-space()='Add Record']")).Click();

            wait.Until(d =>
                d.FindElements(By.XPath(
                    $"//article[contains(@class,'card')]" +
                    $"[.//h3[normalize-space()='{title}']]")).Count > 0);
        }

        private void FilterRecords(string title, string artist)
        {
            var titleInput = driver.FindElement(
                By.CssSelector("input[placeholder='Title']"));

            var artistInput = driver.FindElement(
                By.CssSelector("input[placeholder='Artist']"));

            titleInput.Clear();
            titleInput.SendKeys(title);

            artistInput.Clear();
            artistInput.SendKeys(artist);

            driver.FindElement(By.XPath(
                "//button[normalize-space()='Filter']")).Click();
        }

        private int GetRecordCount()
        {
            return driver.FindElements(By.CssSelector(
                "article.card")).Count;
        }

        // Tests

        [TestMethod]
        public void TestMusicRecordsPage()
        {
            Assert.AreEqual("Vue Template", driver.Title);

            var navbar = driver.FindElement(
                By.ClassName("navbar-brand"));

            Assert.AreEqual("Music Records", navbar.Text);

            Assert.IsTrue(driver.PageSource.Contains(
                "Your collection"));
        }

        [TestMethod]
        public void TestDatabaseStartsEmpty()
        {
            wait.Until(d =>
                d.FindElements(By.XPath(
                    "//h3[normalize-space()='No music records found']"))
                    .Count > 0);

            Assert.AreEqual(0, GetRecordCount());

            Assert.IsTrue(driver.PageSource.Contains(
                "No music records found"));
        }

        [TestMethod]
        public void TestTitleFilterInput()
        {
            var titleInput = driver.FindElement(
                By.CssSelector("input[placeholder='Title']"));

            titleInput.SendKeys("Test Title");

            Assert.AreEqual("Test Title",
                titleInput.GetAttribute("value"));
        }

        [TestMethod]
        public void TestArtistFilterInput()
        {
            var artistInput = driver.FindElement(
                By.CssSelector("input[placeholder='Artist']"));

            artistInput.SendKeys("Test Artist");

            Assert.AreEqual("Test Artist",
                artistInput.GetAttribute("value"));
        }

        [TestMethod]
        public void TestFilterButton()
        {
            var filterButton = driver.FindElement(
                By.XPath("//button[normalize-space()='Filter']"));

            Assert.IsTrue(filterButton.Displayed);
            Assert.IsTrue(filterButton.Enabled);

            filterButton.Click();

            wait.Until(d =>
                d.FindElements(By.XPath(
                    "//h3[normalize-space()='No music records found']"))
                    .Count > 0);
        }

        [TestMethod]
        public void TestLogin()
        {
            Login();

            Assert.IsTrue(driver.FindElement(
                By.XPath("//button[normalize-space()='Logout']"))
                .Displayed);

            Assert.IsTrue(driver.FindElement(
                By.Id("record-title")).Displayed);

            Assert.IsTrue(driver.FindElement(
                By.XPath("//button[normalize-space()='Add Record']"))
                .Displayed);
        }

        [TestMethod]
        public void TestLoginWithWrongPassword()
        {
            driver.FindElement(By.CssSelector(
                "input[placeholder='Username']")).SendKeys("admin");

            driver.FindElement(By.CssSelector(
                "input[placeholder='Password']")).SendKeys("wrongpassword");

            driver.FindElement(By.XPath(
                "//button[normalize-space()='Login']")).Click();

            wait.Until(d =>
                d.FindElements(By.CssSelector(
                    ".navbar .text-warning")).Count > 0);

            Assert.AreEqual(0, driver.FindElements(
                By.Id("record-title")).Count);

            Assert.AreEqual(0, driver.FindElements(
                By.XPath("//button[normalize-space()='Logout']")).Count);
        }

        [TestMethod]
        public void TestAddMusicRecord()
        {
            Login();

            AddRecord("Test Album", "Test Artist", "45", "2024");

            var record = driver.FindElement(By.XPath(
                "//article[contains(@class,'card')]" +
                "[.//h3[normalize-space()='Test Album']]"));

            Assert.IsTrue(record.Text.Contains("Test Album"));
            Assert.IsTrue(record.Text.Contains("Test Artist"));
            Assert.IsTrue(record.Text.Contains("2024"));
            Assert.IsTrue(record.Text.Contains("45 min"));

            Assert.AreEqual(1, GetRecordCount());
        }

        [TestMethod]
        public void TestFilterByTitle()
        {
            Login();

            AddRecord("Rock Album", "Rock Artist", "40", "2023");
            AddRecord("Jazz Album", "Jazz Artist", "50", "2024");

            FilterRecords("Rock Album", "");

            wait.Until(d => GetRecordCount() == 1);

            Assert.IsTrue(driver.PageSource.Contains("Rock Album"));
            Assert.IsFalse(driver.PageSource.Contains("Jazz Album"));
        }

        [TestMethod]
        public void TestFilterByArtist()
        {
            Login();

            AddRecord("Rock Album", "Rock Artist", "40", "2023");
            AddRecord("Jazz Album", "Jazz Artist", "50", "2024");

            FilterRecords("", "Jazz Artist");

            wait.Until(d => GetRecordCount() == 1);

            Assert.IsTrue(driver.PageSource.Contains("Jazz Album"));
            Assert.IsFalse(driver.PageSource.Contains("Rock Album"));
        }

        [TestMethod]
        public void TestFilterNoResults()
        {
            Login();

            AddRecord("Test Album", "Test Artist", "45", "2024");

            FilterRecords("Nonexistent Album", "");

            wait.Until(d =>
                d.FindElements(By.XPath(
                    "//h3[normalize-space()='No music records found']"))
                    .Count > 0);

            Assert.AreEqual(0, GetRecordCount());
        }

        [TestMethod]
        public void TestDeleteMusicRecord()
        {
            Login();

            AddRecord("Delete Test", "Test Artist", "30", "2022");

            Assert.AreEqual(1, GetRecordCount());

            var record = driver.FindElement(By.XPath(
                "//article[contains(@class,'card')]" +
                "[.//h3[normalize-space()='Delete Test']]"));

            record.FindElement(By.XPath(
                ".//button[normalize-space()='Delete']")).Click();

            wait.Until(d => GetRecordCount() == 0);

            Assert.IsTrue(driver.PageSource.Contains(
                "No music records found"));
        }

        [TestMethod]
        public void TestLogout()
        {
            Login();

            driver.FindElement(By.XPath(
                "//button[normalize-space()='Logout']")).Click();

            wait.Until(d =>
                d.FindElements(By.XPath(
                    "//button[normalize-space()='Login']")).Count > 0);

            Assert.AreEqual(0, driver.FindElements(
                By.Id("record-title")).Count);

            Assert.AreEqual(0, driver.FindElements(
                By.XPath("//button[normalize-space()='Add Record']")).Count);
        }
    }
}

