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
            this._validator = validator;
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
                return this.CreateFailedResult(new ErrorData(ErrorCodes.CommandIsNotSet, "Command not set"));
            }

            if (!this.IsValid)
            {
                return this.CreateFailedResult(new ErrorData(ErrorCodes.Validation, "Command not valid"));
            }

            return await this.Processor();
        }

        protected abstract Result<TQueryResult, ErrorData> CreateFailedResult(ErrorData errorData);

        protected abstract Task<Result<TQueryResult, ErrorData>> Processor();
    }
}
