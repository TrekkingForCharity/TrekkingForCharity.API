using System;
using System.Collections.Generic;
using System.Text;
using TrekkingForCharity.Api.Core.Queries;
using TrekkingForCharity.Api.Read.Models;

namespace TrekkingForCharity.Api.Read.QueryResults
{
    public class GetTrekBySlugQueryResult : IQueryResult
    {
        public GetTrekBySlugQueryResult(Trek trek)
        {
            this.Trek = trek;
        }

        public Trek Trek { get; }
    }
}
