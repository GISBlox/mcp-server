// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GISBlox.MCP.Server.Tests
{
    [TestClass]
    public class InfoToolsTests
    {
        private GISBloxClient _client = null!;

        #region Initialization and cleanup

        [TestInitialize]
        public void Init()
        {
            var serviceKey = Environment.GetEnvironmentVariable("GISBLOX_SERVICE_KEY");            
            var serviceUrl = Environment.GetEnvironmentVariable("GISBLOX_SERVICE_URL") ?? "https://services.gisblox.com";

            _client = GISBloxClient.CreateClient(serviceUrl, serviceKey);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_client is IDisposable d)
            {
                d.Dispose();
            }
        }

        #endregion

        [TestMethod]
        public async Task GetSubscriptionInfo()
        {
            List<Subscription> subscriptions = await InfoTools.GetSubscriptions(_client, CancellationToken.None);

            subscriptions.ForEach(sub =>
                Console.WriteLine($"\r\nName: {sub.Name} \r\nDescription: {sub.Description} \r\nRegistration date: {sub.RegisterDate} Expiration date: {sub.ExpirationDate} Expired: {sub.Expired}"));

            Assert.IsNotNull(subscriptions, "Response is null.");
            Assert.AreNotEqual(0, subscriptions.Count, "No subscriptions returned.");
        }
    }
}
