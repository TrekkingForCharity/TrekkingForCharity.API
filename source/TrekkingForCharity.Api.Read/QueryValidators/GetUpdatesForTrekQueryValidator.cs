using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using TrekkingForCharity.Api.Core.Constants;
using TrekkingForCharity.Api.Read.Queries;

namespace TrekkingForCharity.Api.Read.QueryValidators
{
    public class GetUpdatesForTrekQueryValidator : AbstractValidator<GetUpdatesForTrekQuery>
    {
        public GetUpdatesForTrekQueryValidator()
        {
            this.RuleFor(x=>x.TrekId).NotEqual(Guid.Empty).WithErrorCode(ValidationCodes.FieldIsRequired);
        }
    }
}
