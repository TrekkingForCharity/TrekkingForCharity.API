// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Collections.Generic;
using TrekkingForCharity.Api.Write.CommandExecutors;

namespace TrekkingForCharity.Api.App.DataTransport
{
    public class ExecutionResponse
    {
        private readonly List<ValidationError> _errors;

        private ExecutionResponse()
            : this(true)
        {
        }

        private ExecutionResponse(bool succeeded, object successReference = null, string errorCode = null, string failMessage = null)
        {
            this.FailMessage = succeeded ? string.Empty : failMessage ?? string.Empty;
            this.ErrorCode = succeeded ? string.Empty : errorCode ?? string.Empty;
            this.Result = succeeded ? successReference : null;
            this.Success = succeeded;
            this._errors = new List<ValidationError>();
        }

        public string ErrorCode { get; }

        public string FailMessage { get; }

        public dynamic Result { get; }

        public bool Success { get; }

        public IReadOnlyCollection<ValidationError> Errors => this._errors.AsReadOnly();

        public static ExecutionResponse CreateFailedExecutionResponse(string errorCode, string message)
        {
            var executionResponse = new ExecutionResponse(false, errorCode: errorCode, failMessage: message);

            return executionResponse;
        }

        public static ExecutionResponse CreateEmptySuccessfulExecutionResponse()
        {
            return new ExecutionResponse();
        }

        public static ExecutionResponse CreateSuccessfulExecutionResponse(object successReference)
        {
            return new ExecutionResponse(true, successReference);
        }

        public void AddError(string property, string errorCode, string message)
        {
            if (this.Success)
            {
                throw new ArgumentException("Cannot add error to a successful result");
            }

            this._errors.Add(new ValidationError(property, errorCode, message));
        }
    }
}