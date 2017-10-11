// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace TrekkingForCharity.Api.Read
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Update : TableEntity
    {
        [JsonProperty]
        public double Lng { get; set; }

        [JsonProperty]
        public double Lat { get; set; }

        [JsonProperty]
        public string Title { get; set; }

        [JsonProperty]
        public string Message { get; set; }

        [JsonProperty]
        public DateTime WhenCreated { get; set; }

        [JsonProperty]
        public Guid Id => Guid.Parse(this.RowKey);

        [JsonProperty]
        public Guid TrekId => Guid.Parse(this.PartitionKey);
    }
}
