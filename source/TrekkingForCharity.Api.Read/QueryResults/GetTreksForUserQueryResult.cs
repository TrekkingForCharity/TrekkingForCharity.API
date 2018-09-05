using System;
using System.Collections.Generic;
using System.Text;
using TrekkingForCharity.Api.Core.Queries;
using TrekkingForCharity.Api.Read.Models;

namespace TrekkingForCharity.Api.Read.QueryResults
{
    public class GetTreksForUserQueryResult : IQueryResult
    {
        public GetTreksForUserQueryResult(IEnumerable<Trek> treks)
        {
            this.Treks = treks;
        }

        public IEnumerable<Trek> Treks { get; }
    }
}
