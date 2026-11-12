using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Selenium_Test
{
    [TestClass]
    public class UsingSeleniumTest
    {
        [TestMethod]
        public void TestMusicRecordsPage()
        {
            IWebDriver driver = new ChromeDriver();

            driver.Navigate().GoToUrl("http://127.0.0.1:5500");

            // Check page title
            var title = driver.Title;
            Assert.AreEqual("Vue Template", title);

            // Check navbar brand text
            var navbarBrand = driver.FindElement(By.ClassName("navbar-brand"));
            Assert.AreEqual("Music Records", navbarBrand.Text);

            // Wait for records to load and check if list items exist
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
            var recordsList = driver.FindElements(By.ClassName("list-group-item"));

            Assert.IsTrue(recordsList.Count > 0, "Music records should be loaded");

            driver.Quit();
        }
    }
}
