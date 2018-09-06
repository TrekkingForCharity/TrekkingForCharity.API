// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace TrekkingForCharity.Api.Write.Models
{
    public class Update : TableEntity
    {
        public Update()
        {
        }

        public Update(double lng, double lat, string title, string message, Guid trekId)
        {
            this.Lng = lng;
            this.Lat = lat;
            this.Title = title;
            this.Message = message;
            this.RowKey = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            this.PartitionKey = trekId.ToString();
        }

        public double Lng { get; set; }

        public double Lat { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        [IgnoreProperty]
        public int WhenCreated => int.Parse(this.RowKey);

        [IgnoreProperty]
        public Guid TrekId => Guid.Parse(this.PartitionKey);
    }
}