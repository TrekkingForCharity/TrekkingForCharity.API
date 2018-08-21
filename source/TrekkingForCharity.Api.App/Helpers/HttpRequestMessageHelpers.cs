// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System.Net;
using System.Net.Http;
using System.Text;
using FluentValidation.Results;
using Newtonsoft.Json;
using TrekkingForCharity.Api.Write.DataTransport;
using TrekkingForCharity.Api.Write.Helpers;

namespace TrekkingForCharity.Api.App.Helpers
{
    public static class HttpRequestMessageHelpers
    {
        public static HttpResponseMessage CreateApiErrorResponse(string apiErrorCode, string message)
        {
            var executionResult = ExecutionResult.CreateFailedExecutionResult(apiErrorCode, message);
            return CreateResponseCamelCase(executionResult, HttpStatusCode.BadRequest);
        }

        public static HttpResponseMessage CreateApiErrorResponseWithSingleValidationError(
            string property,
            string errorCode, string message)
        {
            var executionResult =
                ExecutionResult.CreateFailedExecutionResultWithSingleValidationError(property, errorCode, message);
            return CreateResponseCamelCase(executionResult, (HttpStatusCode)422);
        }

        public static HttpResponseMessage CreateApiErrorResponseFromValidateResults(ValidationResult validationResult)
        {
            var executionResult = validationResult.ToExecutionResult();
            return CreateResponseCamelCase(executionResult, (HttpStatusCode)422);
        }

        public static HttpResponseMessage CreateEmptySuccessResponseMessage()
        {
            var executionResult = ExecutionResult.CreateEmptySuccessfulExecutionResult();
            return CreateResponseCamelCase(executionResult);
        }

        public static HttpResponseMessage CreateSuccessResponseMessage(object obj)
        {
            var executionResult = ExecutionResult.CreateSuccessfulExecutionResult(obj);
            return CreateResponseCamelCase(executionResult);
        }

        public static HttpResponseMessage CreateResponse(HttpStatusCode httpStatusCode)
        {
            return new HttpResponseMessage(httpStatusCode);
        }

        public static HttpResponseMessage CreateResponseCamelCase(
            object obj,
            HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            return new HttpResponseMessage(httpStatusCode)
            {
                Content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8,
                    "application/json")
            };
        }
    }
}