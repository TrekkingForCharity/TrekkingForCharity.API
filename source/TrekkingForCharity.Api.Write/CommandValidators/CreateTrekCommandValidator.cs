﻿// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;
using Microsoft.WindowsAzure.Storage.Table;
using Slugify;
using TrekkingForCharity.Api.Core.Constants;
using TrekkingForCharity.Api.Core.Helpers;
using TrekkingForCharity.Api.Write.Commands;
using TrekkingForCharity.Api.Write.Models;

namespace TrekkingForCharity.Api.Write.CommandValidators
{
    public class CreateTrekCommandValidator : AbstractValidator<CreateTrekCommand>
    {
        private readonly CloudTable _trekSlugTable;
        private readonly SlugHelper _slugHelper;

        public CreateTrekCommandValidator(CloudTable trekSlugTable, SlugHelper slugHelper)
        {
            this._trekSlugTable = trekSlugTable ?? throw new ArgumentNullException(nameof(trekSlugTable));
            this._slugHelper = slugHelper ?? throw new ArgumentNullException(nameof(slugHelper));

            this.RuleFor(x => x.Name)
                .NotEmpty()
                .WithErrorCode(ValidationCodes.FieldIsRequired)
                .CustomAsync(this.UniqueTrekName);
            this.RuleFor(x => x.Description).NotEmpty().WithErrorCode(ValidationCodes.FieldIsRequired);
        }

        private async Task UniqueTrekName(string name, CustomContext context, CancellationToken cancellationToken)
        {
            var generatedSlug = this._slugHelper.GenerateSlug(name);
            var trekSlugResult = await this._trekSlugTable.RetrieveWithResult<TrekSlug>(generatedSlug.First().ToString(), generatedSlug);
            if (trekSlugResult.IsSuccess)
            {
                var validationFailure =
                    new ValidationFailure(context.PropertyName, "Trek name not unique")
                    {
                        ErrorCode = ValidationCodes.TrekNameInUse
                    };
                context.AddFailure(validationFailure);
            }
        }
    }
}