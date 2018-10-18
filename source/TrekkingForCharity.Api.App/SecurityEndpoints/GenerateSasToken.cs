// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace TrekkingForCharity.Api.App.SecurityEndpoints
{
    public static class GenerateSasToken
    {
        [FunctionName("GenerateSasToken")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Function, methods: "POST", Route = "security/generate-sas-token")]
            HttpRequestMessage req,
            [Blob("demo", FileAccess.Read, Connection = "")]CloudBlobDirectory blobDirectory,
            ILogger log)
        {
            var permissions = SharedAccessBlobPermissions.Write;

            var container = blobDirectory.Container;
            var sasToken = GetContainerSasToken(container, permissions);

            var run = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(sasToken) };
            return run;
        }

        public static string GetContainerSasToken(CloudBlobContainer container, SharedAccessBlobPermissions permissions)
        {
            var adHocSas = new SharedAccessBlobPolicy
            {
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                Permissions = permissions
            };

            var sasContainerToken = container.GetSharedAccessSignature(adHocSas, null);
            return sasContainerToken;
        }
    }
}