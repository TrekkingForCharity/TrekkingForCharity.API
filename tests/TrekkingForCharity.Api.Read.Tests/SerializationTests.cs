using System;
using Newtonsoft.Json;
using Xunit;

namespace TrekkingForCharity.Api.Read.Tests
{
    public class SerializationTests
    {
        [Fact]
        public void TrekShouldSerializeWithoutTableData()
        {
            var trek = new Trek
            {
                PartitionKey = Guid.NewGuid().ToString(),
                RowKey = Guid.NewGuid().ToString(),
            };
            var serializedObject = JsonConvert.SerializeObject(trek).ToLower();
            Assert.DoesNotContain("partitionkey", serializedObject);
            Assert.DoesNotContain("rowkey", serializedObject);
        }

        [Fact]
        public void UpdateShouldSerializeWithoutTableData()
        {
            var update = new Update
            {
                PartitionKey = Guid.NewGuid().ToString(),
                RowKey = 1.ToString(),
            };
            var serializedObject = JsonConvert.SerializeObject(update).ToLower();
            Assert.DoesNotContain("partitionkey", serializedObject);
            Assert.DoesNotContain("rowkey", serializedObject);
        }

        [Fact]
        public void WaypointShouldSerializeWithoutTableData()
        {
            var waypoint = new Waypoint
            {
                PartitionKey = Guid.NewGuid().ToString(),
                RowKey = 1.ToString(),
            };
            var serializedObject = JsonConvert.SerializeObject(waypoint).ToLower();
            Assert.DoesNotContain("partitionkey", serializedObject);
            Assert.DoesNotContain("rowkey", serializedObject);
        }
    }
}
