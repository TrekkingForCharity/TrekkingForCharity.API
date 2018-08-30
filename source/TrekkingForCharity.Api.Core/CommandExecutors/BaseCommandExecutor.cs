// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using FluentValidation;
using System.Threading.Tasks;
using FluentValidation.Results;
using TrekkingForCharity.Api.Core.Commands;
using TrekkingForCharity.Api.Core.Constants;

namespace TrekkingForCharity.Api.Core.CommandExecutors
{
    public abstract class BaseCommandExecutor<TCommand, TCommandReult>
        where TCommand : ICommand
    {
        private readonly IValidator<TCommand> _validator;

        protected BaseCommandExecutor(IValidator<TCommand> validator)
        {
            this._validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public TCommand Command { get; private set; }

        public bool IsValid { get; private set; }

        public async Task<ValidationResult> ValidateAndSetCommand(TCommand cmd)
        {
            this.Command = cmd;
            var result = await this._validator.ValidateAsync(cmd);
            this.IsValid = result.IsValid;
            return result;
        }

        public async Task<TCommandReult> Execute()
        {
            if (this.Command == null)
            {
                return this.CreateFailedResult(new ErrorData(ErrorCodes.CommandIsNotSet, "Command not set"));
            }

            if (!this.IsValid)
            {
                return this.CreateFailedResult(new ErrorData(ErrorCodes.Validation, "Command not valid"));
            }

            return await this.Executor();
        }

        protected abstract TCommandReult CreateFailedResult(ErrorData errorData);

        protected abstract Task<TCommandReult> Executor();
    }
}