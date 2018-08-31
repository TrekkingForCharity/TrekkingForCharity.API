// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using Newtonsoft.Json;
using TrekkingForCharity.Api.Read;
using TrekkingForCharity.Api.Read.Models;
using Xunit;

namespace TrekkingForCharity.Api.Tests.Read
{
    public class SerializationTests
    {
        [Fact]
        public void Should_BeWithoutTableData_When_TrekIsSerialize()
        {
            var trek = new Trek
            {
                PartitionKey = Guid.NewGuid().ToString(),
                RowKey = Guid.NewGuid().ToString()
            };
            var serializedObject = JsonConvert.SerializeObject(trek).ToLower();
            Assert.DoesNotContain("partitionkey", serializedObject);
            Assert.DoesNotContain("rowkey", serializedObject);
        }

        [Fact]
        public void Should_BeWithoutTableData_When_UpdateIsSerialize()
        {
            var update = new Update
            {
                PartitionKey = Guid.NewGuid().ToString(),
                RowKey = 1.ToString()
            };
            var serializedObject = JsonConvert.SerializeObject(update).ToLower();
            Assert.DoesNotContain("partitionkey", serializedObject);
            Assert.DoesNotContain("rowkey", serializedObject);
        }

        [Fact]
        public void Should_BeWithoutTableData_When_WaypointIsSerialize()
        {
            var waypoint = new Waypoint
            {
                PartitionKey = Guid.NewGuid().ToString(),
                RowKey = 1.ToString()
            };
            var serializedObject = JsonConvert.SerializeObject(waypoint).ToLower();
            Assert.DoesNotContain("partitionkey", serializedObject);
            Assert.DoesNotContain("rowkey", serializedObject);
        }
    }
}