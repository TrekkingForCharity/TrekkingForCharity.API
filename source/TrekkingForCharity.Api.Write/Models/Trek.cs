// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace TrekkingForCharity.Api.Write.Models
{
    public class Trek : TableEntity
    {
        public Trek()
        {
        }

        public Trek(string name, string description, string bannerImage, DateTime whenToStart,
            string userRef)
        {
            this.RowKey = Guid.NewGuid().ToString();
            this.PartitionKey = userRef;
            this.Name = name;
            this.Description = description;
            this.BannerImage = bannerImage;
            this.WhenToStart = whenToStart;
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public string BannerImage { get; set; }

        public DateTime WhenToStart { get; set; }

        public DateTime? WhenStarted { get; set; }

        public void UpdateBasicDetails(string name, string description, string bannerImage, DateTime whenToStart)
        {
            this.Name = name;
            this.Description = description;
            this.WhenToStart = whenToStart;
            this.BannerImage = bannerImage;
        }

        public void UpdateBannerImage(string bannerImage)
        {
            this.BannerImage = bannerImage;
        }

        public void Start()
        {
            this.WhenStarted = DateTime.UtcNow;
        }
    }
}