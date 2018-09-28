using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace TrekkingForCharity.Api.Core.Helpers
{
    public static class TableContinuationTokenExtensions
    {
        public static string ToBase64(this TableContinuationToken tableContinuationToken)
        {
            var continuationTokenJson = JsonConvert.SerializeObject(tableContinuationToken);
            var continuationTokenBytes = Encoding.UTF8.GetBytes(continuationTokenJson);
            return Convert.ToBase64String(continuationTokenBytes);
        }
    }

    public static class StringExtensions
    {
        public static TableContinuationToken ToTableContinuationToken(this string continuationTokenBase64)
        {
            var continuationTokenBytes = Convert.FromBase64String(continuationTokenBase64);
            var continuationTokenJson = Encoding.UTF8.GetString(continuationTokenBytes);
            return JsonConvert.DeserializeObject<TableContinuationToken>(continuationTokenJson);
        }
    }
}
