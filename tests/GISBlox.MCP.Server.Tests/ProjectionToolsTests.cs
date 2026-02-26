// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Server.ToolBase;
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
   public class ProjectionToolsTests
   {
      private GISBloxClient _client = null!;
      private ProjectionTools _projectionTools = null!;

      const int API_QUOTA_DELAY = 2500;  // Allows to run all tests together without exceeding API call quota

      #region Initialization and cleanup

      [TestInitialize]
      public void Init()
      {
         var serviceKey = Environment.GetEnvironmentVariable("GISBLOX_SERVICE_KEY");
         var serviceUrl = Environment.GetEnvironmentVariable("GISBLOX_SERVICE_URL") ?? "https://services.gisblox.com";

         _client = GISBloxClient.CreateClient(serviceUrl, serviceKey, applicationName: "GISBlox.MCP.Server.Tests");
         _projectionTools = new ProjectionTools();
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

      #region ToRDS

      [TestMethod]
      public async Task ReprojectToRDS()
      {
         double lat = 51.998929, lon = 4.375587;
         
         McpToolOutput result = await _projectionTools.ToRDSFromCoordinate(_client, lat, lon, CancellationToken.None);
         Assert.IsNotNull(result, "Response is empty.");

         RDPoint? rdPoint = result.Data as RDPoint;
         Assert.IsNotNull(rdPoint, "Response is empty.");
         Assert.IsTrue(rdPoint.X == 85530 && rdPoint.Y == 446100);
      }

      [TestMethod]
      public async Task ReprojectToRDSComplete()
      {
         double lat = 51.998929, lon = 4.375587;
         
         McpToolOutput result = await _projectionTools.ToRDSFromCoordinateComplete(_client, lat, lon, CancellationToken.None);         
         Assert.IsNotNull(result, "Response is empty.");

         Location? location = result.Data as Location;
         Assert.IsNotNull(location, "Response is empty.");
         Assert.IsTrue(location.X == 85530 && location.Y == 446100 && location.Lat == 51.998929 && location.Lon == 4.375587);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task ReprojectToRDSMultiple()
      {
         double[][] coords =
         [
            [51.998929, 4.375587],
            [53.1, 4.2],
            [53.11, 4.3]
         ];
         
         McpToolOutput result = await _projectionTools.ToRDSFromCoordinateList(_client, coords, CancellationToken.None);
         Assert.IsNotNull(result, "Response is empty.");

         List<RDPoint>? rdPoints = result.Data as List<RDPoint>;
         Assert.IsNotNull(rdPoints, "Response is empty.");
         Assert.IsTrue(rdPoints[0].X == 85530 && rdPoints[0].Y == 446100);
         Assert.IsTrue(rdPoints[1].X == 75483 && rdPoints[1].Y == 568787);
         Assert.IsTrue(rdPoints[2].X == 82197 && rdPoints[2].Y == 569794);

         await Task.Delay(API_QUOTA_DELAY * 2, CancellationToken.None);
      }

      [TestMethod]
      public async Task ReprojectToRDSMultipleComplete()
      {
         double[][] coords =
         [
            [51.998929, 4.375587],
            [53.1, 4.2],
            [53.11, 4.3]
         ];
         
         McpToolOutput result = await _projectionTools.ToRDSFromCoordinateListComplete(_client, coords, CancellationToken.None);         
         Assert.IsNotNull(result, "Response is empty.");

         List<Location>? loc = result.Data as List<Location>;
         Assert.IsNotNull(loc, "Response is empty.");
         Assert.IsTrue(loc[0].X == 85530 && loc[0].Y == 446100 && loc[0].Lat == 51.998929 && loc[0].Lon == 4.375587);
         Assert.IsTrue(loc[1].X == 75483 && loc[1].Y == 568787 && loc[1].Lat == 53.1 && loc[1].Lon == 4.2);
         Assert.IsTrue(loc[2].X == 82197 && loc[2].Y == 569794 && loc[2].Lat == 53.11 && loc[2].Lon == 4.3);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      #endregion

      #region ToWGS84

      [TestMethod]
      public async Task ReprojectToWGS84()
      {
         int x = 85530, y = 446100;
         
         McpToolOutput result = await _projectionTools.ToWGS84FromRDPoint(_client, x, y, 6, CancellationToken.None);        // Round the coordinate to 6 digits
         Assert.IsNotNull(result, "Response is empty.");

         Coordinate? coord = result.Data as Coordinate;
         Assert.IsNotNull(coord, "Response is empty.");
         Assert.IsTrue(coord.Lat == 51.998927 && coord.Lon == 4.375584);
      }

      [TestMethod]
      public async Task ReprojectToWGS84Complete()
      {
         int x = 85530, y = 446100;
        
         McpToolOutput result = await _projectionTools.ToWGS84FromRDPointComplete(_client, x, y, -1, CancellationToken.None);  // No rounding
         Assert.IsNotNull(result, "Response is empty.");

         Location? location = result.Data as Location;

         Assert.IsNotNull(location, "Response is empty.");
         Assert.IsTrue(location.Lat == 51.998927449317591 && location.Lon == 4.3755841993518345 && location.X == 85530 && location.Y == 446100);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task ReprojectToWGS84Multiple()
      {
         int[][] points =
         [
            [100000, 555000],
            [1, 2],
            [111000, 550000]
         ];
         
         McpToolOutput result = await _projectionTools.ToWGS84FromRDPointList(_client, points, -1, CancellationToken.None);   // No rounding
         Assert.IsNotNull(result, "Response is empty.");

         List<Coordinate>? coords = result.Data as List<Coordinate>;
         Assert.IsNotNull(coords, "Response is empty.");         

         Assert.IsTrue(coords[0].Lat == 52.9791861737104 && coords[0].Lon == 4.56833613045079);
         Assert.IsNull(coords[1]);
         Assert.IsTrue(coords[2].Lat == 52.93526683092437 && coords[2].Lon == 4.7327735938900535);

         await Task.Delay(API_QUOTA_DELAY * 2, CancellationToken.None);
      }

      [TestMethod]
      public async Task ReprojectToWGS84MultipleComplete()
      {
         int[][] points =
         [
            [100000, 555000],
            [1, 2],
            [111000, 550000]
         ];
        
         McpToolOutput result = await _projectionTools.ToWGS84FromRDPointListComplete(_client, points, 5, CancellationToken.None);   // Round the coordinates to 5 digits         
         Assert.IsNotNull(result, "Response is empty.");

         List<Location>? coords = result.Data as List<Location>;

         Assert.IsNotNull(coords, "Response is empty.");
         Assert.IsTrue(coords[0].Lat == 52.97919 && coords[0].Lon == 4.56834 && coords[0].X == 100000 && coords[0].Y == 555000);
         Assert.IsTrue(coords[1].Lat == 0 && coords[1].Lon == 0 && coords[1].X == -9999 && coords[1].Y == -9999);
         Assert.IsTrue(coords[2].Lat == 52.93527 && coords[2].Lon == 4.73277 && coords[2].X == 111000 && coords[2].Y == 550000);
      }

      #endregion
   }
}