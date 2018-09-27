// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

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
using TrekkingForCharity.Api.Write.Commands;
using TrekkingForCharity.Api.Write.Models;

namespace TrekkingForCharity.Api.Write.CommandExecutors
{
    public class
        UpdateTrekImageCommandExecutor : BaseCommandExecutor<UpdateTrekImageCommand, ResultWithError<ErrorData>>
    {
        private readonly ICurrentUserAccessor _currentUserAccessor;
        private readonly Index _trekIndex;
        private readonly CloudTable _trekTable;

        public UpdateTrekImageCommandExecutor(
            IValidator<UpdateTrekImageCommand> validator,
            ICurrentUserAccessor currentUserAccessor, CloudTable trekTable, Index trekIndex)
            : base(validator)
        {
            this._currentUserAccessor = currentUserAccessor;
            this._trekTable = trekTable;
            this._trekIndex = trekIndex;
        }

        protected override ResultWithError<ErrorData> CreateFailedResult(ErrorData errorData)
        {
            return ResultWithError.Fail(errorData);
        }

        protected override async Task<ResultWithError<ErrorData>> Executor()
        {
            var currentUserMaybe = await this._currentUserAccessor.GetCurrentUser();
            if (currentUserMaybe.HasNoValue)
            {
                return ResultWithError.Fail(new ErrorData(
                    ErrorCodes.NotAuthenticated,
                    "No authenticated user found"));
            }

            await this._trekTable.CreateIfNotExistsAsync();

            var currentUser = currentUserMaybe.Value;

            var result =
                await this._trekTable.RetrieveWithResult<Trek>(currentUser.UserId, this.Command.TrekId.ToString());
            if (result.IsFailure)
            {
                return ResultWithError.Fail(new ErrorData(
                    ErrorCodes.TrekNotFound,
                    $"Trek with Id {this.Command.TrekId} not found"));
            }

            var trek = result.Value;

            trek.UpdateImageUri(this.Command.ImageUri);

            var updateResult = await this._trekTable.UpdateEntity(trek);

            if (updateResult.IsFailure)
            {
                return ResultWithError.Fail(new ErrorData(
                    ErrorCodes.Creation, "Something went wrong when trying to update the trek"));
            }

            await this._trekIndex.AddObjectAsync(JObject.FromObject(new
            {
                objectId = $"{trek.PartitionKey}¬{trek.RowKey}",
                imageUri = trek.ImageUri
            }));

            return ResultWithError.Ok<ErrorData>();
        }
    }
}