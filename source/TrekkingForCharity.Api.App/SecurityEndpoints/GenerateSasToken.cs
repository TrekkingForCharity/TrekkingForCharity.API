// An HTTP trigger Azure Function that returns a SAS token for Azure Storage for the specified container. 
// You can also optionally specify a particular blob name and access permissions. 
// To learn more, see https://github.com/Azure/azure-webjobs-sdk-templates/blob/master/Templates/SasToken-CSharp/readme.md

// Request body format:
// - `ContainerName` - *required*. Name of container in storage account
// - `BlobName` - *optional*. Used to scope permissions to a particular blob
// - `Permission` - *optional*. Default value is read permissions. The format matches the enum values of SharedAccessBlobPermissions. 
//    Possible values are "Read", "Write", "Delete", "List", "Add", "Create". Comma-separate multiple permissions, such as "Read, Write, Create".

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;

namespace TrekkingForCharity.Api.App.SecurityEndpoints
{
    public static class GenerateSasToken
    {
        [FunctionName("GenerateSasToken")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Function, methods: "POST", Route = "security/generate-sas-token")]
            HttpRequestMessage req,
            [Blob("demo", FileAccess.Read, Connection = "")]
            CloudBlobDirectory blobDirectory,
            TraceWriter log)
        {
            var permissions = SharedAccessBlobPermissions.Write;

            var container = blobDirectory.Container;
            var sasToken = GetContainerSasToken(container, permissions);

            return new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent(sasToken)};
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