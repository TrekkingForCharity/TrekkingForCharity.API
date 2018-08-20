// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using Slugify;

namespace TrekkingForCharity.Api.App.Helpers
{
    public static class EntityExtensions
    {
        public static Read.Waypoint ToRead(this Write.Models.Waypoint writeWaypoint)
        {
            var readWaypoint = new Read.Waypoint
            {
                Lng = writeWaypoint.Lng,
                Lat = writeWaypoint.Lng,
                PartitionKey = writeWaypoint.PartitionKey,
                RowKey = writeWaypoint.RowKey,
                WhenReached = writeWaypoint.WhenReached
            };
            return readWaypoint;
        }

        public static Read.Update ToRead(this Write.Models.Update writeUpdate)
        {
            var readUpdate = new Read.Update
            {
                Lng = writeUpdate.Lng,
                Lat = writeUpdate.Lat,
                Message = writeUpdate.Message,
                Title = writeUpdate.Title,
                PartitionKey = writeUpdate.PartitionKey,
                RowKey = writeUpdate.RowKey
            };
            return readUpdate;
        }

        public static Read.Trek ToRead(this Write.Models.Trek writeTrek, ISlugHelper slugHelper)
        {
            var readTrek = new Read.Trek
            {
                BannerImage = writeTrek.BannerImage,
                Description = writeTrek.Description,
                Name = writeTrek.Name,
                WhenStarted = writeTrek.WhenStarted,
                WhenToStart = writeTrek.WhenToStart,
                Slug = slugHelper.GenerateSlug(writeTrek.Name)
            };
            return readTrek;
        }
    }
}