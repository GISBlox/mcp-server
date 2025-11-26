// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.Services.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GISBlox.MCP.Server.Tests
{
   [TestClass]
   public class VisualizationToolsTests
   {
      private GISBloxClient _client = null!;

      #region Initialization and cleanup

      [TestInitialize]
      public void Init()
      {
         var serviceKey = Environment.GetEnvironmentVariable("GISBLOX_SERVICE_KEY");
         var serviceUrl = Environment.GetEnvironmentVariable("GISBLOX_SERVICE_URL") ?? "https://services.gisblox.com";

         _client = GISBloxClient.CreateClient(serviceUrl, serviceKey, applicationName: "GISBlox.MCP.Server.Tests");
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

      #region PC4

      [TestMethod]
      public async Task VisualizePostalCode4()
      {
         string id = "3069";
         string url = await VisualizationTools.VisualizePostalCode4(_client, id, CancellationToken.None);

         Assert.IsFalse(string.IsNullOrWhiteSpace(url), "Response is empty.");

         Console.WriteLine($"GeoJSON URL: {url}");
         OpenInBrowser(url);
      }

      [TestMethod]
      public async Task VisualizePostalCode4Neighbours()
      {
         string id = "2809";
         string url = await VisualizationTools.VisualizePostalCode4Neighbours(_client, id, CancellationToken.None);

         Assert.IsFalse(string.IsNullOrWhiteSpace(url), "Response is empty.");

         Console.WriteLine($"GeoJSON URL: {url}");
         OpenInBrowser(url);
      }

      #endregion

      #region PC6

      [TestMethod]
      public async Task VisualizePostalCode6()
      {
         string id = "2809RA";
         string url = await VisualizationTools.VisualizePostalCode6(_client, id, CancellationToken.None);

         Assert.IsFalse(string.IsNullOrWhiteSpace(url), "Response is empty.");

         Console.WriteLine($"GeoJSON URL: {url}");
         OpenInBrowser(url);
      }

      [TestMethod]
      public async Task VisualizePostalCode6Neighbours()
      {
         string id = "2809RA";
         string url = await VisualizationTools.VisualizePostalCode6Neighbours(_client, id, CancellationToken.None);

         Assert.IsFalse(string.IsNullOrWhiteSpace(url), "Response is empty.");

         Console.WriteLine($"GeoJSON URL: {url}");
         OpenInBrowser(url);
      }

      #endregion

      #region ZipChat Copilot

      [TestMethod]
      public void OpenInZipChatCopilot()
      {
         string id = "2809RA";
         string url = VisualizationTools.AskZipChatCopilot(id);

         Assert.IsFalse(string.IsNullOrWhiteSpace(url), "Response is empty.");
         Console.WriteLine($"ZipChat Copilot URL: {url}");

         OpenInBrowser(url);
      }

      [TestMethod]
      public void OpenNeigboursInZipChatCopilot()
      {
         string id = "2809RA";
         string url = VisualizationTools.AskZipChatCopilot(id, true);

         Assert.IsFalse(string.IsNullOrWhiteSpace(url), "Response is empty.");
         Console.WriteLine($"ZipChat Copilot URL: {url}");

         OpenInBrowser(url);
      }

      #endregion

      #region Private methods

      private static void OpenInBrowser(string url)
      {
         try
         {
            if (url.Length > 2000)
            {
               Console.WriteLine("URL is too long to open automatically. Please copy and paste it into your browser:");
               Console.WriteLine(url);
               return;
            }

            Process.Start(new ProcessStartInfo
            {
               FileName = url,
               UseShellExecute = true
            });
         }
         catch (Exception ex)
         {
            Console.WriteLine($"Could not open browser: {ex.Message}");
         }
      }

      #endregion
   }
}