using System;
using System.Collections.Generic;
using System.Text;
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
        public void Should_BeValidReadEntity_When_ConvertingWaypoint()
        {
            var writeWaypoint = new Waypoint(
                -60,
                80,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Guid.NewGuid()
            );
            writeWaypoint.Hit();
            var readWaypoint = writeWaypoint.ToRead();
            Assert.Equal(writeWaypoint.Lat, readWaypoint.Lat);
            Assert.Equal(writeWaypoint.Lng, readWaypoint.Lng);
            Assert.Equal(writeWaypoint.TrekId, readWaypoint.TrekId);
            Assert.Equal(writeWaypoint.WhenReached, readWaypoint.WhenReached);
            Assert.Equal(writeWaypoint.WhenToReach, readWaypoint.WhenToReach);
        }

        [Fact]
        public void Should_BeValidReadEntity_When_ConvertingUpdate()
        {
            var writeUpdate = new Update(
                -60,
                80,
                "Some Title",
                "Some Message",
                Guid.NewGuid()
            );
            var readUpdate = writeUpdate.ToRead();
            Assert.Equal(writeUpdate.Lat, readUpdate.Lat);
            Assert.Equal(writeUpdate.Lng, readUpdate.Lng);
            Assert.Equal(writeUpdate.TrekId, readUpdate.TrekId);
            Assert.Equal(writeUpdate.WhenCreated, readUpdate.WhenCreated);
            Assert.Equal(writeUpdate.Message, readUpdate.Message);
            Assert.Equal(writeUpdate.Title, readUpdate.Title);
        }

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
                "UserRef"
            );
            writeTrek.Start();
            var readTrek = writeTrek.ToRead(slugHelper.Object);
            Assert.Equal(writeTrek.Description, readTrek.Description);
            Assert.Equal(writeTrek.BannerImage, readTrek.BannerImage);
            Assert.Equal(writeTrek.Name, readTrek.Name);
            Assert.Equal(writeTrek.WhenToStart, readTrek.WhenToStart);
            Assert.Equal(writeTrek.WhenStarted, readTrek.WhenStarted);
            Assert.Equal(new string('*', 10), readTrek.Slug);
        }

    }
}
