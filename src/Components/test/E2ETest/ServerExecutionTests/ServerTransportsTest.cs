// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Threading;
using BasicTestApp;
using BasicTestApp.Reconnection;
using Microsoft.AspNetCore.Components.E2ETest.Infrastructure;
using Microsoft.AspNetCore.Components.E2ETest.Infrastructure.ServerFixtures;
using Microsoft.AspNetCore.E2ETesting;
using OpenQA.Selenium;
using TestServer;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.Components.E2ETest.ServerExecutionTests
{
    public class ServerTransportsTest : ServerTestBase<BasicTestAppServerSiteFixture<PrerenderedStartup>>
    {
        public ServerTransportsTest(
            BrowserFixture browserFixture,
            BasicTestAppServerSiteFixture<PrerenderedStartup> serverFixture,
            ITestOutputHelper output)
            : base(browserFixture, serverFixture, output)
        {
        }

        [Fact]
        public void DefaultTransportsWorksWithWebSockets()
        {
            Navigate("/prerendered/transports");

            Browser.Exists(By.Id("startNormally")).Click();

            Browser.Exists(By.CssSelector(".interactive"));

            AssertLogContainsMessages(
                LogLevel.Debug,
                "Selecting transport 'WebSockets'.",
                "Information: WebSocket connected to",
                "The HttpConnection connected successfully.",
                "Blazor server-side application started.");
        }

        [Fact]
        public void ErrorIfBrowserDoesNotSupportWebSockets()
        {
            Navigate("/prerendered/transports");

            Browser.Exists(By.Id("startWithWebSocketsDisabledInBrowser")).Click();

            Browser.DoesNotExist(By.CssSelector(".interactive"));

            // Ensure debug logs are present
            AssertLogContainsMessages(
                LogLevel.Debug,
                "Skipping transport 'WebSockets' because it is not supported in your environment.");

            AssertLogContainsMessages(
                LogLevel.Severe,
                "Failed to start the connection: Error: Unable to connect to the server with any of the available transports. WebSockets failed: UnsupportedTransportWebSocketsError: 'WebSockets' is not supported in your environment.",
                "Failed to start the circuit.");

            // Ensure error ui is visible
            var errorUiElem = Browser.Exists(By.Id("blazor-error-ui"), TimeSpan.FromSeconds(10));
            Assert.NotNull(errorUiElem);

            var javascript = (IJavaScriptExecutor)Browser;
            var errorMessage = ((string)javascript.ExecuteScript("getErrorText()")).Trim();
            Assert.Equal("Unable to connect, please ensure you are using an updated browser and WebSockets are available.", errorMessage);

            Browser.Equal("block", () => errorUiElem.GetCssValue("display"));
        }


        void AssertLogContainsMessages(LogLevel severity, params string[] messages)
        {
            var log = Browser.Manage().Logs.GetLog(LogType.Browser);
            foreach (var message in messages)
            {
                Assert.Contains(log, entry =>
                {
                    return entry.Level == severity && entry.Message.StartsWith(message, StringComparison.InvariantCulture);
                });
            }
        }
    }
}
