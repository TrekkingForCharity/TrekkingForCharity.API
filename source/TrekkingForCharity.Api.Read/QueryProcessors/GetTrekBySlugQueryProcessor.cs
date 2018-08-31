// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.WindowsAzure.Storage.Table;
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
    public class GetTrekBySlugQueryProcessor : BaseQueryProcessor<GetTrekBySlugQuery, GetTrekBySlugQueryResult>
    {
        private readonly CloudTable _trekSlugTable;
        private readonly CloudTable _trekTable;

        public GetTrekBySlugQueryProcessor(IValidator<GetTrekBySlugQuery> validator, CloudTable trekSlugTable,
            CloudTable trekTable) : base(validator)
        {
            this._trekSlugTable = trekSlugTable;
            this._trekTable = trekTable;
        }

        protected override Result<GetTrekBySlugQueryResult, ErrorData> CreateFailedResult(ErrorData errorData)
        {
            return Result.Fail<GetTrekBySlugQueryResult, ErrorData>(errorData);
        }

        protected override async Task<Result<GetTrekBySlugQueryResult, ErrorData>> Processor()
        {
            var trekSlugResult =
                await this._trekSlugTable.RetrieveWithResult<TrekSlug>(this.Query.Slug.First().ToString(),
                    this.Query.Slug);

            if (trekSlugResult.IsFailure)
            {
                return Result.Fail<GetTrekBySlugQueryResult, ErrorData>(new ErrorData(ErrorCodes.TrekNotFound, ""));
            }

            var trekSlug = trekSlugResult.Value;

            var trekId = trekSlug.TrekRef.Split('¬');

            var trekResult = await this._trekTable.RetrieveWithResult<Trek>(trekId.First(), trekId.Last());

            if (trekResult.IsFailure)
            {
                return Result.Fail<GetTrekBySlugQueryResult, ErrorData>(new ErrorData(ErrorCodes.TrekNotFound, ""));
            }

            return Result.Ok<GetTrekBySlugQueryResult, ErrorData>(new GetTrekBySlugQueryResult(trekResult.Value));
        }
    }
}