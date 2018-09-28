// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace TrekkingForCharity.Api.Read.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Trek : TableEntity
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string Description { get; set; }

        [JsonProperty]
        public string ImageUri { get; set; }

        [JsonProperty]
        public long WhenToStart { get; set; }

        [JsonProperty]
        public long? WhenStarted { get; set; }

        [JsonProperty]
        public Guid Id => Guid.Parse(this.RowKey);

        [JsonProperty]
        public string UserId => this.PartitionKey;

        [JsonProperty]
        public string Slug { get; set; }
    }
}
