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
    public class ProjectionToolsTests
    {
        private GISBloxClient _client = null!;

        const int API_QUOTA_DELAY = 2500;  // Allows to run all tests together without exceeding API call quota

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

        #region ToRDS

        [TestMethod]
        public async Task ReprojectToRDS()
        {
            Coordinate coord = new(51.998929, 4.375587);
            RDPoint rdPoint = await ProjectionTools.ToRDSFromCoordinate(_client, coord, CancellationToken.None);

            Assert.IsNotNull(rdPoint, "Response is empty.");
            Assert.IsTrue(rdPoint.X == 85530 && rdPoint.Y == 446100);
        }

        [TestMethod]
        public async Task ReprojectToRDSComplete()
        {
            Coordinate coord = new(51.998929, 4.375587);
            Location location = await ProjectionTools.ToRDSFromCoordinateComplete(_client, coord, CancellationToken.None);

            Assert.IsNotNull(location, "Response is empty.");
            Assert.IsTrue(location.X == 85530 && location.Y == 446100 && location.Lat == 51.998929 && location.Lon == 4.375587);

            await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
        }

        [TestMethod]
        public async Task ReprojectToRDSMultiple()
        {
            List<Coordinate> coords =
            [
               new Coordinate(51.998929, 4.375587),
            new Coordinate(53.1, 4.2),
            new Coordinate(53.11, 4.3)
            ];
            List<RDPoint> rdPoints = await ProjectionTools.ToRDSFromCoordinateList(_client, coords, CancellationToken.None);

            Assert.IsNotNull(rdPoints.Count != 0, "Response is empty.");
            Assert.IsTrue(rdPoints[0].X == 85530 && rdPoints[0].Y == 446100);
            Assert.IsTrue(rdPoints[1].X == 75483 && rdPoints[1].Y == 568787);
            Assert.IsTrue(rdPoints[2].X == 82197 && rdPoints[2].Y == 569794);

            await Task.Delay(API_QUOTA_DELAY * 2, CancellationToken.None);
        }

        [TestMethod]
        public async Task ReprojectToRDSMultipleComplete()
        {
            List<Coordinate> coords =
            [
               new Coordinate(51.998929, 4.375587),
            new Coordinate(53.1, 4.2),
            new Coordinate(53.11, 4.3)
            ];
            List<Location> loc = await ProjectionTools.ToRDSFromCoordinateListComplete(_client, coords, CancellationToken.None);

            Assert.IsNotNull(loc.Count != 0, "Response is empty.");
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
            RDPoint rdPoint = new(85530, 446100);
            Coordinate coord = await ProjectionTools.ToWGS84FromRDPoint(_client, rdPoint, 6, CancellationToken.None);        // Round the coordinate to 6 digits

            Assert.IsNotNull(coord, "Response is empty.");
            Assert.IsTrue(coord.Lat == 51.998927 && coord.Lon == 4.375584);
        }

        [TestMethod]
        public async Task ReprojectToWGS84Complete()
        {
            RDPoint rdPoint = new(85530, 446100);
            Location location = await ProjectionTools.ToWGS84FromRDPointComplete(_client, rdPoint, -1, CancellationToken.None);  // No rounding

            Assert.IsNotNull(location, "Response is empty.");
            Assert.IsTrue(location.Lat == 51.998927449317591 && location.Lon == 4.3755841993518345 && location.X == 85530 && location.Y == 446100);

            await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
        }

        [TestMethod]
        public async Task ReprojectToWGS84Multiple()
        {
            List<RDPoint> rdPoints =
            [
               new RDPoint(100000, 555000),
            new RDPoint(1, 2),
            new RDPoint(111000, 550000)
            ];
            List<Coordinate> coords = await ProjectionTools.ToWGS84FromRDPointList(_client, rdPoints, -1, CancellationToken.None);   // No rounding

            Assert.IsNotNull(coords.Count != 0, "Response is empty.");
            Assert.IsTrue(coords[0].Lat == 52.9791861737104 && coords[0].Lon == 4.56833613045079);
            Assert.IsTrue(coords[1].Lat == 0 && coords[1].Lon == 0);
            Assert.IsTrue(coords[2].Lat == 52.93526683092437 && coords[2].Lon == 4.7327735938900535);

            await Task.Delay(API_QUOTA_DELAY * 2, CancellationToken.None);
        }

        [TestMethod]
        public async Task ReprojectToWGS84MultipleComplete()
        {
            List<RDPoint> rdPoints =
            [
               new RDPoint(100000, 555000),
            new RDPoint(1, 2),
            new RDPoint(111000, 550000)
            ];
            List<Location> coords = await ProjectionTools.ToWGS84FromRDPointListComplete(_client, rdPoints, 5, CancellationToken.None);   // Round the coordinates to 5 digits

            Assert.IsNotNull(coords.Count != 0, "Response is empty.");
            Assert.IsTrue(coords[0].Lat == 52.97919 && coords[0].Lon == 4.56834 && coords[0].X == 100000 && coords[0].Y == 555000);
            Assert.IsTrue(coords[1].Lat == 0 && coords[1].Lon == 0 && coords[1].X == -9999 && coords[1].Y == -9999);
            Assert.IsTrue(coords[2].Lat == 52.93527 && coords[2].Lon == 4.73277 && coords[2].X == 111000 && coords[2].Y == 550000);
        }

        #endregion
    }
}