// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using TrekkingForCharity.Api.App.Helpers;
using TrekkingForCharity.Api.Core.Constants;
using TrekkingForCharity.Api.Core.Helpers;
using TrekkingForCharity.Api.Write.CommandResult;
using TrekkingForCharity.Api.Write.Commands;
using TrekkingForCharity.Api.Write.CommandValidators;
using TrekkingForCharity.Api.Write.Models;

namespace TrekkingForCharity.Api.App.CommandExecutors
{
    public static class UpdateWaypointCommandExecutor
    {
        [FunctionName("UpdateWaypointCommandExecutor")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "put", Route = "commands/update-waypoint")]
            HttpRequest req,
            [Table("trek")] CloudTable trekTable,
            [Table("waypoint")] CloudTable waypointTable,
            TraceWriter log,
            ExecutionContext context)
        {
            try
            {
                var config = context.GenerateConfigurationRoot();
                var cert = config["Cert"];

                var principleMaybe = req.Headers.GetCurrentPrinciple(cert);
                if (principleMaybe.HasNoValue)
                {
                    return HttpRequestMessageHelpers.CreateResponse(HttpStatusCode.Forbidden);
                }

                var principle = principleMaybe.Value;
                var userId = principle.Claims.First(x => x.Type == "sub").Value;

                var jsonContent = await req.ReadAsStringAsync();
                var cmd = JsonConvert.DeserializeObject<UpdateWaypointCommand>(jsonContent);

                var validator = new UpdateWaypointCommandValidator();
                var validationResult = await validator.ValidateAsync(cmd);
                if (!validationResult.IsValid)
                {
                    return HttpRequestMessageHelpers.CreateApiErrorResponseFromValidateResults(validationResult);
                }

                var trekResult = await trekTable.RetrieveWithResult<Trek>(userId, cmd.TrekId.ToString());
                if (trekResult.IsFailure)
                {
                    return HttpRequestMessageHelpers.CreateResponse(HttpStatusCode.NotFound);
                }

                var result = await waypointTable.RetrieveWithResult<Waypoint>(cmd.TrekId.ToString(), cmd.WaypointId.ToString());
                if (result.IsFailure)
                {
                    return HttpRequestMessageHelpers.CreateApiErrorResponseWithSingleValidationError("TrekId", ErrorCodes.TrekNotFound,
                        $"Trek with Id {cmd.TrekId} not found");
                }

                var waypoint = result.Value;
                await waypointTable.ExecuteAsync(TableOperation.Delete(waypoint));

                waypoint.UpdateBasicDetails(cmd.Lng, cmd.Lat, cmd.WhenToReach);

                var createResult = await waypointTable.CreateEntity(waypoint);

                if (createResult.IsFailure)
                {
                    return HttpRequestMessageHelpers.CreateApiErrorResponse(
                        ErrorCodes.Creation, "Something went wrong when trying to update the trek");
                }

                return HttpRequestMessageHelpers.CreateSuccessResponseMessage(new UpdateWaypointCommandResult());
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return HttpRequestMessageHelpers.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}