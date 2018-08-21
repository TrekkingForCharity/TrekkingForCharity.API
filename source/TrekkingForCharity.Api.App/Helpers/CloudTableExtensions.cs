// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using ResultMonad;

namespace TrekkingForCharity.Api.App.Helpers
{
    public static class CloudTableExtensions
    {
        public static async Task<Result<T, string>> RetrieveWithResult<T>(this CloudTable cloudTable, string partitionKey,
            string rowKey)
            where T : ITableEntity
        {
            var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result = await cloudTable.ExecuteAsync(retrieveOperation);
            if (result.Result == null)
            {
                return Result.Fail<T, string>("Not Found");
            }

            var obj = (T)result.Result;
            if (obj == null)
            {
                return Result.Fail<T, string>("Returned result not in given type");
            }

            return Result.Ok<T, string>(obj);
        }

        public static async Task<ResultWithError<string>> CreateEntity<T>(this CloudTable cloudTable, T entity)
            where T : ITableEntity
        {
            var operation = TableOperation.Insert(entity);
            var result = await cloudTable.ExecuteAsync(operation);
            return result.HttpStatusCode == 204 ? ResultWithError.Ok<string>() : ResultWithError.Fail<string>(result.HttpStatusCode.ToString());
        }

        public static async Task<ResultWithError<string>> UpdateEntity<T>(this CloudTable cloudTable, T entity)
            where T : ITableEntity
        {
            var operation = TableOperation.Merge(entity);
            var result = await cloudTable.ExecuteAsync(operation);
            return result.HttpStatusCode == 204 ? ResultWithError.Ok<string>() : ResultWithError.Fail<string>(result.HttpStatusCode.ToString());
        }

        public static async Task<ResultWithError<string>> DeleteEntity<T>(this CloudTable cloudTable, T entity)
            where T : ITableEntity
        {
            var operation = TableOperation.Delete(entity);
            var result = await cloudTable.ExecuteAsync(operation);
            return result.HttpStatusCode == 204 ? ResultWithError.Ok<string>() : ResultWithError.Fail<string>(result.HttpStatusCode.ToString());
        }
    }
}