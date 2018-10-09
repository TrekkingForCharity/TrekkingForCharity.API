// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentValidation.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ResultMonad;
using TrekkingForCharity.Api.App.DataTransport;
using TrekkingForCharity.Api.Core;
using TrekkingForCharity.Api.Core.Commands;
using TrekkingForCharity.Api.Core.Queries;

namespace TrekkingForCharity.Api.App.Helpers
{
    public static class HttpRequestMessageHelpers
    {
        public static async Task<TCommand> GetCommand<TCommand>(this HttpRequestMessage requestMessage)
            where TCommand : ICommand
        {
            return await GetObjectFromContent<TCommand>(requestMessage);
        }

        public static async Task<TQuery> GetQuery<TQuery>(this HttpRequestMessage requestMessage)
            where TQuery : IQuery
        {
            return await GetObjectFromContent<TQuery>(requestMessage);
        }

        public static HttpResponseMessage CreateResponseMessageFromExecutionResult(
            this HttpRequestMessage requestMessage, ResultWithError<ErrorData> result)
        {
            return result.IsSuccess
                ? requestMessage.CreateEmptySuccessResponseMessage()
                : requestMessage.CreateApiErrorResponse(result.Error);
        }

        public static HttpResponseMessage CreateResponseMessageFromCommandResult<TCommandResult>(
            this HttpRequestMessage requestMessage, Result<TCommandResult, ErrorData> result)
            where TCommandResult : ICommandResult
        {
            return CreateResponseMessageFromExecutionResult(requestMessage, result);
        }

        public static HttpResponseMessage CreateResponseMessageFromQueryResult<TQueryResult>(
            this HttpRequestMessage requestMessage, Result<TQueryResult, ErrorData> result)
            where TQueryResult : IQueryResult
        {
            return CreateResponseMessageFromExecutionResult(requestMessage, result);
        }

        public static HttpResponseMessage CreateApiErrorResponse(this HttpRequestMessage req, ErrorData errorData)
        {
            var executionResult =
                ExecutionResponse.CreateFailedExecutionResponse(errorData.ErrorCode, errorData.ErrorMessage);
            return req.CreateResponseCamelCase(executionResult, HttpStatusCode.BadRequest);
        }

        public static HttpResponseMessage CreateApiErrorResponseFromValidationResults(
            this HttpRequestMessage req,
            ValidationResult validationResult)
        {
            var executionResult = validationResult.ToExecutionResponse();
            return req.CreateResponseCamelCase(executionResult, (HttpStatusCode)422);
        }

        public static HttpResponseMessage CreateEmptySuccessResponseMessage(this HttpRequestMessage req)
        {
            var executionResult = ExecutionResponse.CreateEmptySuccessfulExecutionResponse();
            return req.CreateResponseCamelCase(executionResult);
        }

        public static HttpResponseMessage CreateSuccessResponseMessage(this HttpRequestMessage req, object obj)
        {
            var executionResult = ExecutionResponse.CreateSuccessfulExecutionResponse(obj);
            return req.CreateResponseCamelCase(executionResult);
        }

        public static HttpResponseMessage CreateResponseCamelCase(this HttpRequestMessage req, object obj,
            HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            return new HttpResponseMessage(httpStatusCode)
            {
                Content = new StringContent(JsonConvert.SerializeObject(obj, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }))
            };
        }

        private static HttpResponseMessage CreateResponseMessageFromExecutionResult<TResult>(
            HttpRequestMessage requestMessage, Result<TResult, ErrorData> result)
        {
            return result.IsSuccess
                ? requestMessage.CreateSuccessResponseMessage(result.Value)
                : requestMessage.CreateApiErrorResponse(result.Error);
        }

        private static async Task<TObject> GetObjectFromContent<TObject>(HttpRequestMessage requestMessage)
        {
            var jsonContent = await requestMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TObject>(jsonContent);
        }
    }
}