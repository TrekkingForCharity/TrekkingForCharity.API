// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using Epoch.net;
using Microsoft.WindowsAzure.Storage.Table;

namespace TrekkingForCharity.Api.Write.Models
{
    public class Waypoint : TableEntity
    {
        public Waypoint()
        {
        }

        public Waypoint(double lng, double lat, int whenToHit, Guid trekId)
        {
            this.RowKey = whenToHit.ToString();
            this.PartitionKey = trekId.ToString();
            this.Lng = lng;
            this.Lat = lat;
        }

        public double Lng { get; set; }

        public double Lat { get; set; }

        public int? WhenReached { get; set; }

        [IgnoreProperty]
        public int WhenToReach => int.Parse(this.RowKey);

        [IgnoreProperty]
        public Guid TrekId => Guid.Parse(this.PartitionKey);

        public void Hit()
        {
            this.WhenReached = DateTime.UtcNow.ToEpoch();
        }

        public void UpdateBasicDetails(double lng, double lat, int hitEpoch)
        {
            this.RowKey = hitEpoch.ToString();
            this.Lng = lng;
            this.Lat = lat;
        }
    }
}