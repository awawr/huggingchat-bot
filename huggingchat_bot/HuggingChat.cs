using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace huggingchat_bot
{
    public class HuggingChat
    {
        private readonly EdgeDriver _driver;
        private readonly WebDriverWait _wdWait;
        public string LastResponse = string.Empty;

        public HuggingChat()
        {
            EdgeDriverService service = EdgeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;

            _driver = new EdgeDriver(service);
            _driver.Url = "https://huggingface.co/chat/";
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            _wdWait = new WebDriverWait(_driver, TimeSpan.FromDays(365));
        }

        public void Quit()
        {
            _driver.Quit();
        }

        public string Ask(string text)
        {
        getResponse:
            var element = _driver.FindElement(By.XPath("//textarea[@placeholder='Ask anything']"));
            _driver.ExecuteScript($"arguments[0].value='{text.Replace("'", "\\'").Replace(":", "\\:").Replace(";", "\\;")}';", element);
            element.SendKeys(" ");
            element.SendKeys(Keys.Backspace);
            element.Submit();

            _wdWait.Until(d => d.FindElement(By.XPath("//*[contains(text(), 'Stop generating')]")).Displayed);
            _wdWait.Until(d => !d.FindElement(By.XPath("//*[contains(text(), 'Stop generating')]")).Displayed);

            try
            {
                element = _driver.FindElement(By.XPath("(//div[@class='prose max-w-none dark:prose-invert max-sm:prose-sm prose-headings:font-semibold prose-h1:text-lg prose-h2:text-base prose-h3:text-base prose-pre:bg-gray-800 dark:prose-pre:bg-gray-900'])[last()]"));
                string response = element.Text;
                if (response == LastResponse)
                    throw new Exception();
                LastResponse = response;
                return response;
            }
            catch
            {
                goto getResponse;
            }
        }
    }
}
