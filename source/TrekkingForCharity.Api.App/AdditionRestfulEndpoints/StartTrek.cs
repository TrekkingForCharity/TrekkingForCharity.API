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
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using TrekkingForCharity.Api.App.Helpers;
using TrekkingForCharity.Api.Core.Constants;
using TrekkingForCharity.Api.Write.Models;

namespace TrekkingForCharity.Api.App.AdditionRestfulEndpoints
{
    public static class StartTrek
    {
        [FunctionName("StartTrek")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = "treks/{trekId}/start")] HttpRequestMessage req,
            [Table("trek", Connection = "")] CloudTable trekTable,
            string trekId,
            TraceWriter log)
        {
            try
            {
                var principleMaybe = req.Headers.GetCurrentPrinciple();
                if (principleMaybe.HasNoValue)
                {
                    return req.CreateResponse(HttpStatusCode.Forbidden);
                }

                var principle = principleMaybe.Value;
                var userId = principle.Claims.First(x => x.Type == "sub").Value;

                var result = await trekTable.RetrieveWithResult<Trek>(userId, trekId);
                if (result.IsFailure)
                {
                    return req.CreateApiErrorResponseWithSingleValidationError("TrekId", ErrorCodes.TrekNotFound,
                        $"Trek with Id {trekId} not found");
                }

                var trek = result.Value;

                trek.Start();

                var updateResult = await trekTable.UpdateEntity(trek);

                if (updateResult.IsFailure)
                {
                    return req.CreateApiErrorResponse(
                        ErrorCodes.Creation, "Something went wrong when trying to update the trek");
                }

                return req.CreateEmptySuccessResponseMessage();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}