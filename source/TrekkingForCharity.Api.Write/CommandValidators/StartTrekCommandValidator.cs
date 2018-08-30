using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using TrekkingForCharity.Api.Write.Commands;

namespace TrekkingForCharity.Api.Write.CommandValidators
{
    public class StartTrekCommandValidator : AbstractValidator<StartTrekCommand>
    {
    }
}
