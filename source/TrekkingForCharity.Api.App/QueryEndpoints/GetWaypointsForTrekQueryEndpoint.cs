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
using TrekkingForCharity.Api.App.Helpers;
using TrekkingForCharity.Api.Read.Queries;
using TrekkingForCharity.Api.Read.QueryProcessors;
using TrekkingForCharity.Api.Read.QueryValidators;

namespace TrekkingForCharity.Api.App.QueryEndpoints
{
    public static class GetWaypointsForTrekQueryEndpoint
    {
        [FunctionName("GetWaypointsForTrekQueryEndpoint")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "queries/get-waypoints-for-trek")]
            HttpRequestMessage req,
            [Table("waypoint")] CloudTable waypointTable,
            TraceWriter log,
            ExecutionContext context)
        {
            try
            {
                var validator = new GetWaypointsForTrekQueryValidator();

                var processor = new GetWaypointsForTrekQueryProcessor(validator, waypointTable);

                var cmd = await req.GetQuery<GetWaypointsForTrekQuery>();

                var validationResult = await processor.ValidateAndSetQuery(cmd);
                if (!validationResult.IsValid)
                {
                    return req.CreateApiErrorResponseFromValidationResults(validationResult);
                }

                var result = await processor.Process();

                return req.CreateResponseMessageFromQueryResult(result);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}