// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using FluentValidation;
using TrekkingForCharity.Api.Core.Constants;
using TrekkingForCharity.Api.Write.Commands;

namespace TrekkingForCharity.Api.Write.CommandValidators
{
    public class CreateTrekCommandValidator : AbstractValidator<CreateTrekCommand>
    {
        public CreateTrekCommandValidator()
        {
            this.RuleFor(x => x.Name)
                .NotEmpty()
                .WithErrorCode(ValidationCodes.FieldIsRequired);
            this.RuleFor(x => x.Description).NotEmpty().WithErrorCode(ValidationCodes.FieldIsRequired);
        }
    }
}