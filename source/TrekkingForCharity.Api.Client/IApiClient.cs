// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResultMonad;
using TrekkingForCharity.Api.Client.Models;

namespace TrekkingForCharity.Api.Client
{
    public interface IApiClient
    {
        Task<Result<Trek>> GetTrek(Guid trekId);

        Task<Result<ExecutionResult, ExecutionResult>> PostTrek(string name, string description, DateTime whenToStart,
            string bannerImage);

        Task<Result<ICollection<Waypoint>>> GetWaypointsForTrek(Guid trekId);

        Task<Result<ICollection<Update>>> GetUpdatesForTrek(Guid trekId);

        Task<Result<ExecutionResult, ExecutionResult>> PutTrek(Guid trekId, string name, string description,
            DateTime whenToStart, string bannerImage);

        Task<Result<ExecutionResult, ExecutionResult>> PostWaypoint(Guid trekId, double lng, double lat,
            DateTime whenToHit);

        Task<Result<ExecutionResult, ExecutionResult>> PutWaypoint(Guid trekId, Guid waypointId, double lng, double lat,
            DateTime whenToHit);

        Task<Result<ExecutionResult, ExecutionResult>> DeleteWaypoint(Guid trekId, Guid waypointId);

        Task<Result<ExecutionResult, ExecutionResult>> PostUpdate(Guid trekId, double lng, double lat, string title,
            string message);

        Task<Result<ExecutionResult, ExecutionResult>> HitWaypoint(Guid trekId, Guid waypointId);

        Task<Result<ExecutionResult, ExecutionResult>> StartTrek(Guid trekId);
    }
}