using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System.Net;
using System.Text.Json;

namespace Duo.Tests
{
    public class BaseTest
    {
        protected IWebDriver driver;
        protected WebDriverWait wait;
        private const string CookiesFile = "cookies.json";

        [SetUp]
        public void SetUp()
        {
            driver = new EdgeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            driver.Navigate().GoToUrl("https://www.duolingo.com/lesson/unit/10/legendary/2");

            LoadCookies();
            driver.Navigate().Refresh();
        }

        protected void SaveCookies()
        {
            var cookies = driver.Manage().Cookies.AllCookies;
            var cookieList = new List<Dictionary<string, object>>();

            foreach (var cookie in cookies)
            {
                cookieList.Add(new Dictionary<string, object>
                {
                    { "name", cookie.Name },
                    { "value", cookie.Value },
                    { "domain", cookie.Domain },
                    { "path", cookie.Path },
                    { "expiry", cookie.Expiry?.ToString("o") },
                    { "secure", cookie.Secure },
                    { "httpOnly", cookie.IsHttpOnly }
                });
            }

            File.WriteAllText(CookiesFile, JsonSerializer.Serialize(cookieList));
        }

        protected void LoadCookies()
        {
            if (File.Exists(CookiesFile))
            {
                var cookieData = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(CookiesFile));

                foreach (var cookieDict in cookieData)
                {
                    var cookie = new OpenQA.Selenium.Cookie(
                        cookieDict["name"].ToString(),
                        cookieDict["value"].ToString(),
                        cookieDict["domain"].ToString(),
                        cookieDict["path"].ToString(),
                        cookieDict.ContainsKey("expiry") && cookieDict["expiry"] != null
                            ? DateTime.Parse(cookieDict["expiry"].ToString())
                            : null
                    );

                    driver.Manage().Cookies.AddCookie(cookie);
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
        }
    }
}