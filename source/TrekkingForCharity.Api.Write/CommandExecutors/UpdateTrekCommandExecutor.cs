using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.WindowsAzure.Storage.Table;
using ResultMonad;
using Slugify;
using TrekkingForCharity.Api.Core.CommandExecutors;
using TrekkingForCharity.Api.Core.Constants;
using TrekkingForCharity.Api.Core.Helpers;
using TrekkingForCharity.Api.Core.Infrastructure;
using TrekkingForCharity.Api.Write.Commands;
using TrekkingForCharity.Api.Write.Models;

namespace TrekkingForCharity.Api.Write.CommandExecutors
{
    public class UpdateTrekCommandExecutor : BaseCommandExecutor<UpdateTrekCommand, ResultWithError<ErrorData>>
    {
        private readonly ICurrentUserAccessor _currentUserAccessor;
        private readonly CloudTable _trekTable;


        public UpdateTrekCommandExecutor(IValidator<UpdateTrekCommand> validator = null, ICurrentUserAccessor currentUserAccessor = null, CloudTable trekTable = null) : base(validator)
        {
            this._currentUserAccessor = currentUserAccessor ?? throw new ArgumentNullException(nameof(currentUserAccessor));
            this._trekTable = trekTable ?? throw new ArgumentNullException(nameof(trekTable));
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

            trek.UpdateBasicDetails(this.Command.Description, this.Command.BannerImage, this.Command.WhenToStart);

            var updateResult = await this._trekTable.UpdateEntity(trek);

            if (updateResult.IsFailure)
            {
                return ResultWithError.Fail(new ErrorData(
                    ErrorCodes.Creation, "Something went wrong when trying to update the trek"));
            }

            return ResultWithError.Ok<ErrorData>();
        }
    }
}
