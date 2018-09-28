// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using ResultMonad;
using TrekkingForCharity.Api.Core.Constants;

namespace TrekkingForCharity.Api.Core.Queries
{
    public abstract class BaseQueryProcessor<TQuery, TQueryResult>
        where TQuery : IQuery
        where TQueryResult : IQueryResult
    {
        private readonly IValidator<TQuery> _validator;

        protected BaseQueryProcessor(IValidator<TQuery> validator)
        {
            this._validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public TQuery Query { get; private set; }

        public bool IsValid { get; private set; }

        public async Task<ValidationResult> ValidateAndSetQuery(TQuery query)
        {
            this.Query = query;
            var result = await this._validator.ValidateAsync(query);
            this.IsValid = result.IsValid;
            return result;
        }

        public async Task<Result<TQueryResult, ErrorData>> Process()
        {
            if (this.Query == null)
            {
                return this.CreateFailedResult(new ErrorData(ErrorCodes.QueryIsNotSet, "Query not set"));
            }

            if (!this.IsValid)
            {
                return this.CreateFailedResult(new ErrorData(ErrorCodes.Validation, "Query not valid"));
            }

            return await this.Processor();
        }

        protected Result<TQueryResult, ErrorData> CreateFailedResult(ErrorData errorData)
        {
            return Result.Fail<TQueryResult, ErrorData>(errorData);
        }

        protected abstract Task<Result<TQueryResult, ErrorData>> Processor();
    }
}
