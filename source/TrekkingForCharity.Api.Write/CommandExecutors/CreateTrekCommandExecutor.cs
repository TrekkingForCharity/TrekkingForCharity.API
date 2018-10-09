// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Threading.Tasks;
using Algolia.Search;
using FluentValidation;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
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
    public class CreateTrekCommandExecutor : BaseCommandExecutor<CreateTrekCommand,
        Result<CreateTrekCommandResult, ErrorData>>
    {
        private readonly ICurrentUserAccessor _currentUserAccessor;
        private readonly Index _trekIndex;
        private readonly CloudTable _trekTable;

        public CreateTrekCommandExecutor(
            IValidator<CreateTrekCommand> validator,
            CloudTable trekTable,
            ICurrentUserAccessor currentUserAccessor,
            Index trekIndex)
            : base(validator)
        {
            this._trekTable = trekTable ?? throw new ArgumentNullException(nameof(trekTable));
            this._currentUserAccessor =
                currentUserAccessor ?? throw new ArgumentNullException(nameof(currentUserAccessor));
            this._trekIndex = trekIndex ?? throw new ArgumentNullException(nameof(trekIndex));
        }

        protected override Result<CreateTrekCommandResult, ErrorData> CreateFailedResult(ErrorData errorData)
        {
            return Result.Fail<CreateTrekCommandResult, ErrorData>(errorData);
        }

        protected override async Task<Result<CreateTrekCommandResult, ErrorData>> Executor()
        {
            await this._trekTable.CreateIfNotExistsAsync();

            var currentUserMaybe = await this._currentUserAccessor.GetCurrentUser();
            if (currentUserMaybe.HasNoValue)
            {
                return Result.Fail<CreateTrekCommandResult, ErrorData>(new ErrorData(
                    ErrorCodes.NotAuthenticated,
                    "No authenticated user found"));
            }

            var currentUser = currentUserMaybe.Value;

            var trek = new Trek(this.Command.Name, this.Command.Description, this.Command.WhenToStart,
                currentUser.UserId);

            var result = await this._trekTable.CreateEntity(trek);
            if (result.IsFailure)
            {
                return Result.Fail<CreateTrekCommandResult, ErrorData>(new ErrorData(
                    ErrorCodes.Creation,
                    "Something went wrong when trying to create the trek"));
            }

            await this._trekIndex.AddObjectAsync(JObject.FromObject(new
            {
                objectId = $"{trek.PartitionKey}¬{trek.RowKey}",
                trekId = trek.RowKey,
                userId = trek.PartitionKey,
                whenToStart = trek.WhenToStart,
                name = trek.Name
            }));

            return Result.Ok<CreateTrekCommandResult, ErrorData>(
                new CreateTrekCommandResult(Guid.Parse(trek.RowKey)));
        }
    }
}