using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace TrekkingForCharity.Api.Write.Models
{
    public class TrekSlug : TableEntity
    {
        public TrekSlug(char initial, string slug, string trekRef)
        {
            this.PartitionKey = initial.ToString();
            this.RowKey = slug;
            this.TrekRef = trekRef;
        }

        public string TrekRef { get; set; }
    }
}