using NUnit.Framework;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using Duo.Tests;
using System.Reflection;
using NUnitLite;

namespace Duo
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                LoginTests loginTests = new();
                loginTests.SetUp();

                try
                {
                    loginTests.TestSuccessfulLogin();
                    loginTests.StartHistory();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
                finally
                {
                    loginTests.TearDown();
                }
            }
        }

        [TestFixture]
        public class LoginTests : BaseTest
        {
            public static void ExecuteWithRetry(Action action, int maxAttempts = 5)
            {
                Thread.Sleep(1000);
                int attempts = 0;
                while (attempts < maxAttempts)
                {
                    try
                    {
                        attempts++;
                        action();
                        break;
                    }
                    catch
                    {
                        if (attempts >= maxAttempts)
                        {
                            Console.WriteLine("Достигнуто максимальное количество попыток.");
                            throw;
                        }
                    }
                }
            }


            [Test]
            public void TestSuccessfulLogin()
            {
                try
                {
                    IWebElement loggedInElement = driver.FindElement(By.CssSelector("button._3fmUm._1rcV8._1VYyp._1ursp._7jW2t._25H5A"));
                }
                catch (NoSuchElementException)
                {
                    IWebElement usernameInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("web-ui1")));
                    IWebElement passwordInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("web-ui2")));

                    usernameInput.SendKeys("");// Email
                    passwordInput.SendKeys("");// Password

                    IWebElement loginButton = driver.FindElement(By.CssSelector("button._3fmUm._1rcV8._1VYyp"));
                    loginButton.Click();

                    SaveCookies();

                    Thread.Sleep(3000);
                }
            }

            private readonly Dictionary<string, string> wordPairs = new()
        {
            { "новая", "new" },
            { "дома", "at home" },
            { "летающий", "flying" },
            { "теперь", "now" },
            { "нужно", "need" },
            { "хочешь", "want" },
            { "но", "but" },
            { "идём", "are going" },
            { "большой", "big" },
            { "ты прав", "you're right" },
            { "робот", "robot" },
            { "смотреть", "to watch" },
            { "скучный", "boring" },
            { "футбольный матч", "football game" },
            { "торговый центр", "mall" }
        };

            [Test]
            public void StartHistory()
            {
                driver.Navigate().GoToUrl("https://www.duolingo.com/lesson/unit/10/legendary/2");

                ExecuteWithRetry(() => ClickStartStoryButton(), 3);

                string nextStepStoryButtonSelector = "button._1rcV8._1VYyp._1ursp._7jW2t._6HDYi._2CoFd._2oGJR._2AtRJ";

                ExecuteWithRetry(() => NextStepStoryButtonClick(nextStepStoryButtonSelector, 4), 3);
                ExecuteWithRetry(() => StoryButtonSelector("li._25kWt", "Нет, это неправда."), 3);

                ExecuteWithRetry(() => NextStepStoryButtonClick(nextStepStoryButtonSelector, 2), 3);
                ExecuteWithRetry(() => StoryButtonSelector("li.w9AVU", "you don't need it"), 3);

                ExecuteWithRetry(() => NextStepStoryButtonClick(nextStepStoryButtonSelector, 2), 3);
                ExecuteWithRetry(() => StoryButtonSelectorBySpanText("li._25kWt", "athome", "div._15RU4.phrase span"), 3);

                ExecuteWithRetry(() => NextStepStoryButtonClick(nextStepStoryButtonSelector, 2), 3);
                ExecuteWithRetry(() => ClickButtonsByText(new string[] { "So", "can we", "go home" }), 3);

                ExecuteWithRetry(() => NextStepStoryButtonClick(nextStepStoryButtonSelector, 5), 3);
                ExecuteWithRetry(() => StoryButtonSelector("li._25kWt._1nWdI", "...используют слово «нужно» вместо слова «хочу»."), 3);

                ExecuteWithRetry(() => NextStepStoryButtonClick(nextStepStoryButtonSelector, 1), 3);
                ExecuteWithRetry(() => ClickAllMatchingButtonPairs(wordPairs, "li._2cEsU", "span[data-test='challenge-tap-token-text']"), 3);

                ExecuteWithRetry(() => NextStepStoryButtonClick(nextStepStoryButtonSelector, 1), 3);
                ExecuteWithRetry(() => NextStepStoryButtonClick("button._1rcV8._1VYyp._1ursp._7jW2t.Kw43R._2AtRJ", 1), 3);

                TearDown();
            }

            private void ClickStartStoryButton()
            {
                IWebElement startStoryButton = WaitForElementClickable(By.CssSelector("button._3fmUm._1rcV8._1VYyp._1ursp._7jW2t._25H5A"));
                startStoryButton.Click();
            }
            private void NextStepStoryButtonClick(string selector, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    IWebElement element = WaitForElementClickable(By.CssSelector(selector));
                    element.Click();
                }
            }
            private void StoryButtonSelector(string storyButtonSelector, string keyword)
            {
                IList<IWebElement> storyButtons = WaitForElementsVisible(By.CssSelector(storyButtonSelector));
                IWebElement? targetButton = null;

                foreach (IWebElement storyButton in storyButtons)
                {
                    if (storyButton.Text.Contains(keyword))
                    {
                        targetButton = storyButton.FindElement(By.CssSelector("button"));
                        break;
                    }
                }

                if (targetButton != null)
                {
                    WaitForElementClickable(targetButton).Click();
                }
            }
            private void StoryButtonSelectorBySpanText(string listItemSelector, string targetText, string spanText)
            {
                IList<IWebElement> listItems = WaitForElementsVisible(By.CssSelector(listItemSelector));
                IWebElement? targetButton = null;

                foreach (IWebElement listItem in listItems)
                {
                    IList<IWebElement> spans = listItem.FindElements(By.CssSelector(spanText));

                    string combinedText = string.Join("", spans.Select(span => span.Text));

                    if (combinedText.Equals(targetText, StringComparison.OrdinalIgnoreCase))
                    {
                        targetButton = listItem.FindElement(By.CssSelector("button[data-test='stories-choice']"));
                        break;
                    }
                }

                if (targetButton != null)
                {
                    WaitForElementClickable(targetButton).Click();
                }
            }
            private void ClickButtonsByText(string[] buttonTexts)
            {
                Thread.Sleep(2000);
                foreach (var text in buttonTexts)
                {
                    var button = driver.FindElement(By.XPath($"//button[.//span[@data-test='challenge-tap-token-text' and text()='{text}']]"));
                    button.Click();
                }
            }
            private void ClickAllMatchingButtonPairs(Dictionary<string, string> wordPairs, string listItemSelector, string spanTextSelector)
            {
                IList<IWebElement> listItems = WaitForElementsVisible(By.CssSelector(listItemSelector));
             
                HashSet<int> clickedIndexes = new();

                for (int i = 0; i < listItems.Count; i++)
                {
                    if (clickedIndexes.Contains(i))
                        continue;

                    IWebElement firstItem = listItems[i];
                    IWebElement firstSpan = firstItem.FindElement(By.CssSelector(spanTextSelector));
                    string firstText = firstSpan.Text;

                    if (!wordPairs.ContainsKey(firstText))
                    {
                        Console.WriteLine($"❌ Перевод для '{firstText}' не найден в словаре.");
                        continue;
                    }

                    string translation = wordPairs[firstText];

                    IWebElement firstButton = firstItem.FindElement(By.CssSelector("button"));
                    WaitForElementClickable(firstButton).Click();

                    // Look for the element with the translation  
                    for (int j = 0; j < listItems.Count; j++)
                    {
                        if (i == j || clickedIndexes.Contains(j))
                            continue;

                        IWebElement listItem = listItems[j];
                        IWebElement spanElement = listItem.FindElement(By.CssSelector(spanTextSelector));
                        string buttonText = spanElement.Text;

                        if (buttonText == translation)
                        {
                            IWebElement button = listItem.FindElement(By.CssSelector("button"));
                            WaitForElementClickable(button).Click();

                            clickedIndexes.Add(i);
                            clickedIndexes.Add(j);
                            break;
                        }
                    }
                }
            }

            private IWebElement WaitForElementVisible(By by) => wait.Until(ExpectedConditions.ElementIsVisible(by));
            private IWebElement WaitForElementClickable(By by) => wait.Until(ExpectedConditions.ElementToBeClickable(by));
            private IWebElement WaitForElementClickable(IWebElement element) => wait.Until(ExpectedConditions.ElementToBeClickable(element));
            private IList<IWebElement> WaitForElementsVisible(By by) => wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(by));

            [TearDown]
            public void TearDown()
            {
                driver?.Quit();
            }
        }
    }
}

//Duo Automation Testing Suite — это набор автоматизированных тестов для веб-приложения Duolingo с использованием Selenium WebDriver и NUnit. Тесты предназначены для проверки функционала авторизации и взаимодействия с уроками (stories), обеспечивая стабильность и надёжность пользовательских сценариев.