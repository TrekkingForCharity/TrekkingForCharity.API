using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using TrekkingForCharity.Api.App.Helpers;
using TrekkingForCharity.Api.App.Infrastructure;
using TrekkingForCharity.Api.Write.CommandExecutors;
using TrekkingForCharity.Api.Write.Commands;
using TrekkingForCharity.Api.Write.CommandValidators;

namespace TrekkingForCharity.Api.App.CommandEndpoints
{
    public static class UpdateTrekImageCommandEndpoint
    {
        [FunctionName("UpdateTrekImageCommandEndpoint")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "commands/update-trek-image")]
            HttpRequestMessage req,
            [Table("trek", Connection = "")] CloudTable trekTable,
            TraceWriter log,
            ExecutionContext context)
        {
            try
            {
                var config = context.GenerateConfigurationRoot();

                var currentUserAccessor = new CurrentUserAccessor(config, req);

                var validator = new UpdateTrekImageCommandValidator();

                var executor = new UpdateTrekImageCommandExecutor(validator, currentUserAccessor, trekTable);

                var cmd = await req.GetCommand<UpdateTrekImageCommand>();

                var validationResult = await executor.ValidateAndSetCommand(cmd);
                if (!validationResult.IsValid)
                {
                    return req.CreateApiErrorResponseFromValidationResults(validationResult);
                }

                var result = await executor.Execute();

                return req.CreateResponseMessageFromExecutionResult(result);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
