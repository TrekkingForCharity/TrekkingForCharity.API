// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System.Linq;
using FluentValidation.Results;
using TrekkingForCharity.Api.Core.Constants;
using TrekkingForCharity.Api.Write.Helpers;
using Xunit;

namespace TrekkingForCharity.Api.Tests.Write.Helpers
{
    public class ValidationExtensionsTests
    {
        [Fact]
        public void Should_HaveValidationErrors_When_CreatedWithValidationFailure()
        {
            var validationResult = new ValidationResult();
            validationResult.Errors.Add(new ValidationFailure("Name", "ERR-001"));
            var er = validationResult.ToExecutionResult();
            Assert.Equal(ErrorCodes.Validation, er.ErrorCode);
            Assert.Equal("validation", er.FailMessage);
            Assert.False(er.Success);
            Assert.Equal(1, er.Errors.Count);
            var error = er.Errors.First();
            Assert.Equal("Name", error.Property);
            Assert.Equal("ERR-001", error.Message);
            Assert.Null(er.Result);
        }
    }
}