// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.WindowsAzure.Storage.Table;
using ResultMonad;
using TrekkingForCharity.Api.Core.CommandExecutors;
using TrekkingForCharity.Api.Core.Constants;
using TrekkingForCharity.Api.Core.Helpers;
using TrekkingForCharity.Api.Core.Infrastructure;
using TrekkingForCharity.Api.Write.Commands;
using TrekkingForCharity.Api.Write.Models;

namespace TrekkingForCharity.Api.Write.CommandExecutors
{
    public class DeleteWaypointCommandExecutor : BaseCommandExecutor<DeleteWaypointCommand, ResultWithError<ErrorData>>
    {
        private readonly ICurrentUserAccessor _currentUserAccessor;
        private readonly CloudTable _trekTable;
        private readonly CloudTable _waypointTable;

        public DeleteWaypointCommandExecutor(
            IValidator<DeleteWaypointCommand> validator,
            ICurrentUserAccessor currentUserAccessor,
            CloudTable trekTable,
            CloudTable waypointTable)
            : base(validator)
        {
            this._currentUserAccessor =
                currentUserAccessor ?? throw new ArgumentNullException(nameof(currentUserAccessor));
            this._trekTable = trekTable ?? throw new ArgumentNullException(nameof(trekTable));
            this._waypointTable = waypointTable ?? throw new ArgumentNullException(nameof(waypointTable));
        }

        protected override ResultWithError<ErrorData> CreateFailedResult(ErrorData errorData)
        {
            return ResultWithError.Fail(errorData);
        }

        protected override async Task<ResultWithError<ErrorData>> Executor()
        {
            var currentUserMaybe = this._currentUserAccessor.GetCurrentUser();
            if (currentUserMaybe.HasNoValue)
            {
                return ResultWithError.Fail(new ErrorData(
                    ErrorCodes.NotAuthenticated,
                    "No authenticated user found"));
            }

            await this._trekTable.CreateIfNotExistsAsync();
            await this._waypointTable.CreateIfNotExistsAsync();

            var currentUser = currentUserMaybe.Value;

            var trekResult =
                await this._trekTable.RetrieveWithResult<Trek>(currentUser.UserId, this.Command.TrekId.ToString());
            if (trekResult.IsFailure)
            {
                return ResultWithError.Fail(new ErrorData(
                    ErrorCodes.TrekNotFound,
                    $"Trek with Id {this.Command.TrekId} not found"));
            }

            var result = await this._waypointTable.RetrieveWithResult<Waypoint>(
                this.Command.TrekId.ToString(),
                this.Command.WaypointId.ToString());
            if (result.IsFailure)
            {
                return ResultWithError.Fail(new ErrorData(
                    ErrorCodes.WaypointNotFound,
                    $"Waypoint with Id {this.Command.WaypointId} not found"));
            }

            var waypoint = result.Value;

            var updateResult = await this._waypointTable.DeleteEntity(waypoint);

            if (updateResult.IsFailure)
            {
                return ResultWithError.Fail(new ErrorData(
                    ErrorCodes.Deletion,
                    "Something went wrong when trying to create the waypoint"));
            }

            return ResultWithError.Ok<ErrorData>();
        }
    }
}