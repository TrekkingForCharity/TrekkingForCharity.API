// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Slugify;
using TrekkingForCharity.Api.App.Helpers;
using TrekkingForCharity.Api.App.Infrastructure;
using TrekkingForCharity.Api.Write.CommandExecutors;
using TrekkingForCharity.Api.Write.Commands;
using TrekkingForCharity.Api.Write.CommandValidators;

namespace TrekkingForCharity.Api.App.CommandEndpoints
{
    public static class CreateTrekCommandEndpoint
    {
        [FunctionName("CreateTrekCommandEndpoint")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "commands/create-trek")]
            HttpRequestMessage req,
            [Table("trek")] CloudTable trekTable,
            [Table("trekslug")] CloudTable trekSlugTable,
            TraceWriter log,
            ExecutionContext context)
        {
            try
            {
                var config = context.GenerateConfigurationRoot();

                var currentUserAccessor = new CurrentUserAccessor(config, req);

                var slugifyHelper = new SlugHelper();
                var validator = new CreateTrekCommandValidator(trekSlugTable, slugifyHelper);

                var executor = new CreateTrekCommandExecutor(validator, trekSlugTable, slugifyHelper, trekTable,
                    currentUserAccessor);

                var cmd = await req.GetCommand<CreateTrekCommand>();

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