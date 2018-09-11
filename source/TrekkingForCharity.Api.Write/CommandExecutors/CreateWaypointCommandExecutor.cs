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
using TrekkingForCharity.Api.Core;
using TrekkingForCharity.Api.Core.Commands;
using TrekkingForCharity.Api.Core.Constants;
using TrekkingForCharity.Api.Core.Helpers;
using TrekkingForCharity.Api.Core.Infrastructure;
using TrekkingForCharity.Api.Write.CommandResult;
using TrekkingForCharity.Api.Write.Commands;
using TrekkingForCharity.Api.Write.Models;

namespace TrekkingForCharity.Api.Write.CommandExecutors
{
    public class CreateWaypointCommandExecutor : BaseCommandExecutor<CreateWaypointCommand,
        Result<CreateWaypointCommandResult, ErrorData>>
    {
        private readonly ICurrentUserAccessor _currentUserAccessor;
        private readonly CloudTable _trekTable;
        private readonly CloudTable _waypointTable;

        public CreateWaypointCommandExecutor(IValidator<CreateWaypointCommand> validator, CloudTable trekTable,
            ICurrentUserAccessor currentUserAccessor, CloudTable waypointTable)
            : base(validator)
        {
            this._trekTable = trekTable ?? throw new ArgumentNullException(nameof(trekTable));
            this._currentUserAccessor = currentUserAccessor ?? throw new ArgumentNullException(nameof(currentUserAccessor));
            this._waypointTable = waypointTable ?? throw new ArgumentNullException(nameof(waypointTable));
        }

        protected override Result<CreateWaypointCommandResult, ErrorData> CreateFailedResult(ErrorData errorData)
        {
            return Result.Fail<CreateWaypointCommandResult, ErrorData>(errorData);
        }

        protected override async Task<Result<CreateWaypointCommandResult, ErrorData>> Executor()
        {
            var currentUserMaybe = await this._currentUserAccessor.GetCurrentUser();
            if (currentUserMaybe.HasNoValue)
            {
                return Result.Fail<CreateWaypointCommandResult, ErrorData>(new ErrorData(
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
                return Result.Fail<CreateWaypointCommandResult, ErrorData>(new ErrorData(
                    ErrorCodes.TrekNotFound,
                    $"Trek with Id {this.Command.TrekId} not found"));
            }

            var update = new Waypoint(this.Command.Lng, this.Command.Lat, this.Command.WhenToReach,
                this.Command.TrekId);

            var result = await this._waypointTable.CreateEntity(update);
            if (result.IsFailure)
            {
                return Result.Fail<CreateWaypointCommandResult, ErrorData>(new ErrorData(
                    ErrorCodes.Creation,
                    "Something went wrong when trying to create the waypoint"));
            }

            return Result.Ok<CreateWaypointCommandResult, ErrorData>(new CreateWaypointCommandResult());
        }
    }
}