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
using Slugify;
using TrekkingForCharity.Api.App.Helpers;
using TrekkingForCharity.Api.Core.Constants;
using TrekkingForCharity.Api.Core.Helpers;
using TrekkingForCharity.Api.Write.Commands;
using TrekkingForCharity.Api.Write.CommandValidators;
using TrekkingForCharity.Api.Write.Models;

namespace TrekkingForCharity.Api.App.RestfulEndpoints
{
    public static class PostTrek
    {
        [FunctionName("PostTrek")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = "treks")] HttpRequest req,
            [Table("trek")] CloudTable trekTable,
            [Table("trek-slug")] CloudTable trekSlugTable,
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
                var cmd = JsonConvert.DeserializeObject<CreateTrekCommand>(jsonContent);

                var validator = new CreateTrekCommandValidator(trekSlugTable);
                var validationResult = await validator.ValidateAsync(cmd);
                if (!validationResult.IsValid)
                {
                    return HttpRequestMessageHelpers.CreateApiErrorResponseFromValidateResults(validationResult);
                }

                await trekTable.CreateIfNotExistsAsync();
                await trekSlugTable.CreateIfNotExistsAsync();

                var slugify = new Slugify.SlugHelper();
                var slug = slugify.GenerateSlug(cmd.Name);

                var slugResult = await trekSlugTable.RetrieveWithResult<TrekSlug>(slug.First().ToString(), slug);

                if (slugResult.IsSuccess)
                {
                    return HttpRequestMessageHelpers.CreateApiErrorResponse(
                        ErrorCodes.TrekNameInUse, "The name is used by another trek");
                }

                var trek = new Trek(cmd.Name, cmd.Description, cmd.BannerImage, cmd.WhenToStart, userId);

                var result = await trekTable.CreateEntity(trek);
                if (result.IsFailure)
                {
                    return HttpRequestMessageHelpers.CreateApiErrorResponse(
                        ErrorCodes.Creation, "Something went wrong when trying to create the trek");
                }

                var trekSlug = new TrekSlug(slug.First(), slug, $"{trek.PartitionKey}¬{trek.RowKey}");
                result = await trekSlugTable.CreateEntity(trekSlug);
                if (result.IsFailure)
                {
                    return HttpRequestMessageHelpers.CreateApiErrorResponse(
                        ErrorCodes.Creation, "Something went wrong when trying to create the trek");
                }

                var slugHelper = new SlugHelper();
                return HttpRequestMessageHelpers.CreateSuccessResponseMessage(trek.ToRead(slugHelper));
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return HttpRequestMessageHelpers.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}