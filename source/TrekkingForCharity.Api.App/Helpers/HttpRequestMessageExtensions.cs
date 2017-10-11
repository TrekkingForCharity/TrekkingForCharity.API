// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using FluentValidation.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TrekkingForCharity.Api.Write.DataTransport;
using TrekkingForCharity.Api.Write.Helpers;

namespace TrekkingForCharity.Api.App.Helpers
{
    public static class HttpRequestMessageExtensions
    {
        public static HttpResponseMessage CreateApiErrorResponse(this HttpRequestMessage req, string apiErrorCode, string message)
        {
            var executionResult = ExecutionResult.CreateFailedExecutionResult(apiErrorCode, message);
            return req.CreateResponse(HttpStatusCode.BadRequest, executionResult);
        }

        public static HttpResponseMessage CreateApiErrorResponseWithSingleValidationError(this HttpRequestMessage req, string property, string errorCode, string message)
        {
            var executionResult = ExecutionResult.CreateFailedExecutionResultWithSingleValidationError(property, errorCode, message);
            return req.CreateResponseCamelCase(executionResult, (HttpStatusCode)422);
        }

        public static HttpResponseMessage CreateApiErrorResponseFromValidateResults(this HttpRequestMessage req, ValidationResult validationResult)
        {
            var executionResult = validationResult.ToExecutionResult();
            return req.CreateResponseCamelCase(executionResult, (HttpStatusCode)422);
        }

        public static HttpResponseMessage CreateEmtptySuccessResponseMessage(this HttpRequestMessage req)
        {
            var executionResult = ExecutionResult.CreateEmptySuccessfulExecutionResult();
            return req.CreateResponseCamelCase(executionResult);
        }

        public static HttpResponseMessage CreateResponseCamelCase(this HttpRequestMessage req, object obj, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            return req.CreateResponse(httpStatusCode, obj, new JsonMediaTypeFormatter
            {
                SerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                },
                UseDataContractJsonSerializer = false
            });
        }
    }
}