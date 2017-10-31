// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Refit;
using ResultMonad;
using TrekkingForCharity.Api.Client.Models;

namespace TrekkingForCharity.Api.Client
{
    public class ApiClient : IApiClient
    {
        private readonly IApiInternal _apiInternal;

        public ApiClient(IApiConfiguration apiConfiguration)
        {
            this._apiInternal = RestService.For<IApiInternal>(
                new HttpClient(new ApiClientHttpClientHandler(apiConfiguration.GetToken,apiConfiguration.ApiKey))
                {
                    BaseAddress = new Uri(apiConfiguration.BaseUrl)
                });
        }

        public async Task<Result<Trek>> GetTrek(Guid trekId)
        {
            var responseMessage = await this._apiInternal.GetTrek(trekId);
            return await this.ProcessHttpResponseMessageAsGet<Trek>(responseMessage);
        }

        public async Task<Result<ExecutionResult, ExecutionResult>> PostTrek(string name, string description, DateTime whenToStart, string bannerImage)
        {
            var responseMessage = await this._apiInternal.PostTrek(new Trek
            {
                Name = name,
                Description = description,
                WhenToStart = whenToStart,
                BannerImage = bannerImage
            });

            return await this.ProcessHttpResponseMessageAsPost(responseMessage);
        }

        public async Task<Result<ICollection<Waypoint>>> GetWaypointsForTrek(Guid trekId)
        {
            var responseMessage = await this._apiInternal.GetWaypointsForTrek(trekId);
            return await this.ProcessHttpResponseMessageAsGet<ICollection<Waypoint>>(responseMessage);
        }

        public async Task<Result<ICollection<Update>>> GetUpdatesForTrek(Guid trekId)
        {
            var responseMessage = await this._apiInternal.GetUpdatesForTrek(trekId);
            return await this.ProcessHttpResponseMessageAsGet<ICollection<Update>>(responseMessage);
        }

        public async Task<Result<ExecutionResult, ExecutionResult>> PutTrek(Guid trekId, string name, string description, DateTime whenToStart, string bannerImage)
        {
            var responseMessage = await this._apiInternal.PutTrek(trekId, new Trek
            {
                Name = name,
                Description = description,
                WhenToStart = whenToStart,
                BannerImage = bannerImage
            });

            return await this.ProcessHttpResponseMessageAsPost(responseMessage);
        }

        public async Task<Result<ExecutionResult, ExecutionResult>> PostWaypoint(Guid trekId, double lng, double lat, DateTime whenToHit)
        {
            var responseMessage = await this._apiInternal.PostWaypoint(trekId, new Waypoint
            {
                Lng = lng,
                Lat = lat,
                WhenToHit = whenToHit
            });

            return await this.ProcessHttpResponseMessageAsPost(responseMessage);
        }

        public async Task<Result<ExecutionResult, ExecutionResult>> PutWaypoint(Guid trekId, Guid waypointId, double lng, double lat, DateTime whenToHit)
        {
            var responseMessage = await this._apiInternal.PutWaypoint(trekId, waypointId, new Waypoint
            {
                Lng = lng,
                Lat = lat,
                WhenToHit = whenToHit
            });

            return await this.ProcessHttpResponseMessageAsPost(responseMessage);
        }

        public async Task<Result<ExecutionResult, ExecutionResult>> DeleteWaypoint(Guid trekId, Guid waypointId)
        {
            var responseMessage = await this._apiInternal.DeleteWaypoint(trekId, waypointId);

            return await this.ProcessHttpResponseMessageAsPost(responseMessage);
        }

        public async Task<Result<ExecutionResult, ExecutionResult>> PostUpdate(Guid trekId, double lng, double lat, string title, string message)
        {
            var responseMessage = await this._apiInternal.PostUpdate(trekId, new Update
            {
                Lng = lng,
                Lat = lat,
                Message = message,
                Title = title
            });

            return await this.ProcessHttpResponseMessageAsPost(responseMessage);
        }

        public async Task<Result<ExecutionResult, ExecutionResult>> HitWaypoint(Guid trekId, Guid waypointId)
        {
            var responseMessage = await this._apiInternal.HitWaypoint(trekId, waypointId);

            return await this.ProcessHttpResponseMessageAsPost(responseMessage);
        }

        public async Task<Result<ExecutionResult, ExecutionResult>> StartTrek(Guid trekId)
        {
            var responseMessage = await this._apiInternal.StartTrek(trekId);

            return await this.ProcessHttpResponseMessageAsPost(responseMessage);
        }

        private async Task<Result<T>> ProcessHttpResponseMessageAsGet<T>(HttpResponseMessage responseMessage)
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                var content = await responseMessage.Content.ReadAsStringAsync();
                return Result.Ok(JsonConvert.DeserializeObject<T>(content));
            }

            if (responseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                return Result.Fail<T>();
            }

            throw new ApiException();
        }

        private async Task<Result<ExecutionResult, ExecutionResult>> ProcessHttpResponseMessageAsPost(HttpResponseMessage responseMessage)
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                return Result.Ok<ExecutionResult, ExecutionResult>(
                    await this.DeserializeHttpResponseMessage(responseMessage));
            }

            if (responseMessage.StatusCode == HttpStatusCode.BadRequest || responseMessage.StatusCode == (HttpStatusCode)411)
            {
                return Result.Fail<ExecutionResult, ExecutionResult>(await this.DeserializeHttpResponseMessage(responseMessage));
            }

            throw new ApiException();
        }

        private async Task<ExecutionResult> DeserializeHttpResponseMessage(HttpResponseMessage responseMessage)
        {
            var content = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ExecutionResult>(content);
        }
    }
}