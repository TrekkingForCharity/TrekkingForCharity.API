using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.WindowsAzure.Storage.Table;
using ResultMonad;
using TrekkingForCharity.Api.Core;
using TrekkingForCharity.Api.Core.Queries;
using TrekkingForCharity.Api.Read.Queries;
using TrekkingForCharity.Api.Read.QueryResults;

namespace TrekkingForCharity.Api.Read.QueryProcessors
{
    public class GetUpdatesForTrekQueryProcessor : BaseQueryProcessor<GetUpdatesForTrekQuery, GetUpdatesForTrekQueryResult>
    {
        public readonly CloudTable _updateTable;
        public GetUpdatesForTrekQueryProcessor(IValidator<GetUpdatesForTrekQuery> validator, CloudTable updateTable) : base(validator)
        {
            this._updateTable = updateTable ?? throw new ArgumentNullException(nameof(updateTable));
        }

        protected override async Task<Result<GetUpdatesForTrekQueryResult, ErrorData>> Processor()
        {
            throw new NotImplementedException();
        }
    }
}
