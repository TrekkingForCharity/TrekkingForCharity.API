// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System.Collections.Generic;

namespace TrekkingForCharity.Api.Write.DataTransport
{
    public class ExecutionResult
    {
        private readonly List<ValidationError> _errors;

        private ExecutionResult()
            : this(true)
        {
        }

        private ExecutionResult(bool suceeded, dynamic successReference = null, string errorCode = null, string failMessage = null)
        {
            this.FailMessage = suceeded ? string.Empty : failMessage;
            this.ErrorCode = suceeded ? string.Empty : errorCode;
            this.Result = suceeded ? successReference : null;
            this.Success = suceeded;
            this._errors = new List<ValidationError>();
        }

        public string ErrorCode { get; }

        public string FailMessage { get; }

        public dynamic Result { get; }

        public bool Success { get; }

        public IReadOnlyCollection<ValidationError> Errors => this._errors.AsReadOnly();

        public static ExecutionResult CreateFailedExecutionResult(string errorCode, string message)
        {
            var executionResult = new ExecutionResult(false, errorCode: errorCode, failMessage: message);

            return executionResult;
        }

        public static ExecutionResult CreateFailedExecutionResultWithSingleValidationError(
            string property, string errorCode, string message)
        {
            var executionResult = new ExecutionResult(false);
            executionResult.AddError(property, errorCode, message);
            return executionResult;
        }

        public static ExecutionResult CreateEmptySuccessfulExecutionResult()
        {
            return new ExecutionResult();
        }

        public static ExecutionResult CreateSuccessfulExecutionResult(dynamic successReference)
        {
            return new ExecutionResult(true, successReference);
        }

        public void AddError(string property, string errorCode, string message)
        {
            this._errors.Add(new ValidationError(property, errorCode, message));
        }
    }
}