
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Selenium_Test
{
    [TestClass]
    [DoNotParallelize]
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

            ((IJavaScriptExecutor)driver).ExecuteScript(
                "window.localStorage.clear(); window.sessionStorage.clear();");
            driver.Navigate().Refresh();

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

            // Wait until login completes
            wait.Until(d =>
                d.FindElements(By.XPath(
                    "//button[normalize-space()='Logout']")).Count > 0);

            // Verify that the authentication token exists
            var token = (string)((IJavaScriptExecutor)driver)
                .ExecuteScript("return localStorage.getItem('token');");

            Assert.IsFalse(
                string.IsNullOrEmpty(token),
                "Login succeeded in the UI, but no authentication token was stored.");
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
            {
                var records = d.FindElements(By.XPath(
                    $"//article[contains(@class,'card')]" +
                    $"[.//h3[normalize-space()='{title}']]"));

                if (records.Count > 0)
                    return true;

                var errors = d.FindElements(
                    By.CssSelector("form.card .alert-danger"));

                if (errors.Count > 0)
                {
                    Assert.Fail("API error: " + errors[0].Text);
                }

                return false;
            });
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

            // Enter a title that should not exist
            var titleInput = driver.FindElement(
                By.CssSelector("input[placeholder='Title']"));

            titleInput.Clear();
            titleInput.SendKeys("NonexistentRecord123456789");

            // Click Filter
            filterButton.Click();

            // Wait for the empty-state message
            wait.Until(d =>
                d.FindElements(By.XPath(
                    "//h3[normalize-space()='No music records found']"))
                    .Count > 0);

            // Verify that no records are displayed
            Assert.AreEqual(0, GetRecordCount());
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

            string title = "Test Album " + Guid.NewGuid().ToString("N");

            AddRecord(title, "Test Artist", "45", "2024");

            var record = driver.FindElement(By.XPath(
                "//article[contains(@class,'card')]" +
                $"[.//h3[normalize-space()='{title}']]"));

            Assert.IsTrue(record.Text.Contains(title));
            Assert.IsTrue(record.Text.Contains("Test Artist"));
            Assert.IsTrue(record.Text.Contains("2024"));
            Assert.IsTrue(record.Text.Contains("45 sec"));

            Assert.AreEqual(1, driver.FindElements(By.XPath(
                "//article[contains(@class,'card')]" +
                $"[.//h3[normalize-space()='{title}']]")).Count);
        }



        [TestMethod]
        public void TestFilterByTitle()
        {
            Login();

            // Create unique titles
            string rockTitle = "Rock Album " + Guid.NewGuid().ToString("N");
            string jazzTitle = "Jazz Album " + Guid.NewGuid().ToString("N");

            // Add two records
            AddRecord(rockTitle, "Rock Artist", "40", "2023");
            AddRecord(jazzTitle, "Jazz Artist", "50", "2024");

            // Filter by the unique Rock Album title
            FilterRecords(rockTitle, "");

            // Wait for exactly one matching record
            wait.Until(d =>
                d.FindElements(By.XPath(
                    "//article[contains(@class,'card')]" +
                    $"[.//h3[normalize-space()='{rockTitle}']]"))
                    .Count == 1
                && GetRecordCount() == 1);

            // Verify the correct record is displayed
            var record = driver.FindElement(By.CssSelector("article.card"));

            Assert.IsTrue(record.Text.Contains(rockTitle));

            // Verify Jazz Album is not displayed
            Assert.AreEqual(0, driver.FindElements(By.XPath(
                "//article[contains(@class,'card')]" +
                $"[.//h3[normalize-space()='{jazzTitle}']]")).Count);
        }


        [TestMethod]
        public void TestFilterByArtist()
        {
            Login();

            string rockArtist = "Rock Artist " + Guid.NewGuid().ToString("N");
            string jazzArtist = "Jazz Artist " + Guid.NewGuid().ToString("N");

            string rockTitle = "Rock Album " + Guid.NewGuid().ToString("N");
            string jazzTitle = "Jazz Album " + Guid.NewGuid().ToString("N");

            AddRecord(rockTitle, rockArtist, "40", "2023");
            AddRecord(jazzTitle, jazzArtist, "50", "2024");

            FilterRecords("", jazzArtist);

            wait.Until(d =>
                GetRecordCount() == 1 &&
                d.FindElements(By.XPath(
                    "//article[contains(@class,'card')]" +
                    $"[.//h3[normalize-space()='{jazzTitle}']]"))
                    .Count == 1);

            var record = driver.FindElement(By.CssSelector("article.card"));

            Assert.IsTrue(record.Text.Contains(jazzArtist));
            Assert.IsTrue(record.Text.Contains(jazzTitle));

            Assert.AreEqual(0, driver.FindElements(By.XPath(
                "//article[contains(@class,'card')]" +
                $"[.//h3[normalize-space()='{rockTitle}']]")).Count);
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

            // Give the test record a unique title
            string title = "Delete Test " + Guid.NewGuid().ToString("N");

            AddRecord(title, "Test Artist", "30", "2022");

            var recordBy = By.XPath(
                "//article[contains(@class,'card')]" +
                $"[.//h3[normalize-space()='{title}']]");

            // Verify the new record exists
            Assert.AreEqual(1, driver.FindElements(recordBy).Count);

            // Delete the record
            var record = driver.FindElement(recordBy);

            record.FindElement(By.XPath(
                ".//button[normalize-space()='Delete']")).Click();

            // Wait until that specific record is removed
            wait.Until(d =>
                d.FindElements(recordBy).Count == 0);

            // Verify the record is gone
            Assert.AreEqual(0, driver.FindElements(recordBy).Count);
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

