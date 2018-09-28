// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using ResultMonad;
using TrekkingForCharity.Api.Core;
using TrekkingForCharity.Api.Core.Constants;
using TrekkingForCharity.Api.Core.Helpers;
using TrekkingForCharity.Api.Core.Queries;
using TrekkingForCharity.Api.Read.Models;
using TrekkingForCharity.Api.Read.Queries;
using TrekkingForCharity.Api.Read.QueryResults;

namespace TrekkingForCharity.Api.Read.QueryProcessors
{
    public class
        GetUpdatesForTrekQueryProcessor : BaseQueryProcessor<GetUpdatesForTrekQuery, GetUpdatesForTrekQueryResult>
    {
        private readonly CloudTable _updateTable;

        public GetUpdatesForTrekQueryProcessor(IValidator<GetUpdatesForTrekQuery> validator, CloudTable updateTable) :
            base(validator)
        {
            this._updateTable = updateTable ?? throw new ArgumentNullException(nameof(updateTable));
        }

        protected override async Task<Result<GetUpdatesForTrekQueryResult, ErrorData>> Processor()
        {
            TableContinuationToken continuationToken = null;
            if (!string.IsNullOrWhiteSpace(this.Query.ContinuationToken))
            {
                continuationToken = this.Query.ContinuationToken.ToTableContinuationToken();
            }

            var updateResult =
                await this._updateTable.RetrievePagedWithResult<Update>(this.Query.TrekId.ToString(),
                    continuationToken);

            if (!updateResult.IsSuccess)
            {
                return Result.Fail<GetUpdatesForTrekQueryResult, ErrorData>(new ErrorData(ErrorCodes.UpdatesNotFound,
                    $"Issue getting updates for Trek {this.Query.TrekId}"));
            }


            GetUpdatesForTrekQueryResult toReturn;
            var resultTuple = updateResult.Value;
            if (resultTuple.Item2 != null)
            {
                toReturn = new GetUpdatesForTrekQueryResult(resultTuple.Item1, resultTuple.Item2.ToBase64());
            }
            else
            {
                toReturn = new GetUpdatesForTrekQueryResult(resultTuple.Item1);
            }

            return Result.Ok<GetUpdatesForTrekQueryResult, ErrorData>(toReturn);
        }
    }
}