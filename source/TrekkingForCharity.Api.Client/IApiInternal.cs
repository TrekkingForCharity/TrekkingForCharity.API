// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using TrekkingForCharity.Api.Client.Models;

namespace TrekkingForCharity.Api.Client
{
    internal interface IApiInternal
    {
        [Get("/treks/{trekId}")]
        Task<HttpResponseMessage> GetTrek(Guid trekId);

        [Get("/treks/{trekId}/waypoints")]
        Task<HttpResponseMessage> GetWaypointsForTrek(Guid trekId);

        [Get("/treks/{trekId}/updates")]
        Task<HttpResponseMessage> GetUpdatesForTrek(Guid trekId);

        [Post("/treks")]
        Task<HttpResponseMessage> PostTrek([Body]Trek trek, [Header("Authorization")] string authorization = "bearer");

        [Put("treks/{trekId}")]
        Task<HttpResponseMessage> PutTrek(Guid trekId, [Body]Trek trek, [Header("Authorization")] string authorization = "bearer");

        [Post("/treks/{trekId}/waypoints")]
        Task<HttpResponseMessage> PostWaypoint(Guid trekId, [Body]Waypoint waypoint, [Header("Authorization")] string authorization = "bearer");

        [Put("/treks/{trekId}/waypoints/{waypointId}")]
        Task<HttpResponseMessage> PutWaypoint(Guid trekId, Guid waypointId, [Body]Waypoint waypoint, [Header("Authorization")] string authorization = "bearer");

        [Delete("/treks/{trekId}/wayoints/{waypointId}")]
        Task<HttpResponseMessage> DeleteWaypoint(Guid trekId, Guid waypointId, [Header("Authorization")] string authorization = "bearer");

        [Post("/treks/{trekId}/updates")]
        Task<HttpResponseMessage> PostUpdate(Guid trekId, [Body]Update update, [Header("Authorization")] string authorization = "bearer");

        [Put("/treks/{trekId}/updates/{updateId}")]
        Task<HttpResponseMessage> PutUpdate(Guid trekId, Guid updateId, [Body]Update waypoint, [Header("Authorization")] string authorization = "bearer");

        [Post("/treks/{trekId}/waypoints/{waypointId}/hit")]
        Task<HttpResponseMessage> HitWaypoint(Guid trekId, Guid waypointId, [Header("Authorization")] string authorization = "bearer");

        [Post("/treks/{trekId}/start")]
        Task<HttpResponseMessage> StartTrek(Guid trekId, [Header("Authorization")] string authorization = "bearer");
    }
}