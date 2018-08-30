// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using Moq;
using Slugify;
using TrekkingForCharity.Api.App.Helpers;
using TrekkingForCharity.Api.Write.Models;
using Xunit;

namespace TrekkingForCharity.Api.Tests.App.Helpers
{
    public class EntityExtensionsTests
    {
        [Fact]
        public void Should_BeValidReadEntity_When_ConvertingTrek()
        {
            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));

            var writeTrek = new Trek(
                "Trek Name",
                "Trek Description",
                "Banner Image UrlKey",
                DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                "UserRef");
            writeTrek.Start();
            var readTrek = writeTrek.ToRead(slugHelper.Object);
            Assert.Equal(writeTrek.Description, readTrek.Description);
            Assert.Equal(writeTrek.BannerImage, readTrek.BannerImage);
            Assert.Equal(writeTrek.Name, readTrek.Name);
            Assert.Equal(writeTrek.WhenToStart, readTrek.WhenToStart);
            Assert.Equal(writeTrek.WhenStarted, readTrek.WhenStarted);
            Assert.Equal(new string('*', 10), readTrek.Slug);
        }

        [Fact]
        public void Should_BeValidReadEntity_When_ConvertingUpdate()
        {
            var writeUpdate = new Update(
                -60,
                80,
                "Some Title",
                "Some Message",
                Guid.NewGuid());
            var readUpdate = writeUpdate.ToRead();
            Assert.Equal(writeUpdate.Lat, readUpdate.Lat);
            Assert.Equal(writeUpdate.Lng, readUpdate.Lng);
            Assert.Equal(writeUpdate.TrekId, readUpdate.TrekId);
            Assert.Equal(writeUpdate.WhenCreated, readUpdate.WhenCreated);
            Assert.Equal(writeUpdate.Message, readUpdate.Message);
            Assert.Equal(writeUpdate.Title, readUpdate.Title);
        }

        [Fact]
        public void Should_BeValidReadEntity_When_ConvertingWaypoint()
        {
            var writeWaypoint = new Waypoint(
                -60,
                80,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Guid.NewGuid());
            writeWaypoint.Hit();
            var readWaypoint = writeWaypoint.ToRead();
            Assert.Equal(writeWaypoint.Lat, readWaypoint.Lat);
            Assert.Equal(writeWaypoint.Lng, readWaypoint.Lng);
            Assert.Equal(writeWaypoint.TrekId, readWaypoint.TrekId);
            Assert.Equal(writeWaypoint.WhenReached, readWaypoint.WhenReached);
            Assert.Equal(writeWaypoint.WhenToReach, readWaypoint.WhenToReach);
        }
    }
}