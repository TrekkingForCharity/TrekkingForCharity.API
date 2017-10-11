// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace TrekkingForCharity.Api.Write.Models
{
    public class Waypoint : TableEntity
    {
        public Waypoint()
        {
        }

        public Waypoint(double lng, double lat, DateTime whenToHit, Guid trekId)
        {
            this.RowKey = Guid.NewGuid().ToString();
            this.PartitionKey = trekId.ToString();
            this.Lng = lng;
            this.Lat = lat;
            this.WhenToHit = whenToHit;
        }

        public double Lng { get; set; }

        public double Lat { get; set; }

        public DateTime WhenToHit { get; set; }

        public DateTime? WhenHit { get; set; }

        [IgnoreProperty]
        public Guid Id => Guid.Parse(this.RowKey);

        [IgnoreProperty]
        public Guid TrekId => Guid.Parse(this.PartitionKey);

        public void UpdateBasicDetails(double lng, double lat, DateTime whenToHit)
        {
            this.Lng = lng;
            this.Lat = lat;
            this.WhenToHit = whenToHit;
        }

        public void Hit()
        {
            this.WhenHit = DateTime.UtcNow;
        }
    }
}