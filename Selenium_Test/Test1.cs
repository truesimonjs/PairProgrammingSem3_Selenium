
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Selenium_Test
{
    [TestClass]
    public class UsingSeleniumTest
    {
        private static IWebDriver driver;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");

            driver = new ChromeDriver(options);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            driver.Quit();
        }

        [TestInitialize]
        public void OpenPage()
        {
            driver.Navigate().GoToUrl("http://127.0.0.1:5500");
        }

        [TestMethod]
        public void TestMusicRecordsPage()
        {
            Assert.AreEqual("Vue Template", driver.Title);

            var navbar = driver.FindElement(By.ClassName("navbar-brand"));
            Assert.AreEqual("Music Records", navbar.Text);
        }

        [TestMethod]
        public void TestTitleFilterInput()
        {
            var titleInput = driver.FindElement(
                By.CssSelector("input[placeholder='Title']"));

            titleInput.SendKeys("Test Title");

            Assert.AreEqual("Test Title", titleInput.GetAttribute("value"));
        }

        [TestMethod]
        public void TestArtistFilterInput()
        {
            var artistInput = driver.FindElement(
                By.CssSelector("input[placeholder='Artist']"));

            artistInput.SendKeys("Test Artist");

            Assert.AreEqual("Test Artist", artistInput.GetAttribute("value"));
        }

        [TestMethod]
        public void TestFilterButton()
        {
            var filterButton = driver.FindElement(
                By.XPath("//button[normalize-space()='Filter']"));

            Assert.IsTrue(filterButton.Displayed);
            Assert.IsTrue(filterButton.Enabled);

            filterButton.Click();
        }
    }
}

