// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Linq;
using TrekkingForCharity.Api.Write.Commands;
using TrekkingForCharity.Api.Write.CommandValidators;
using Xunit;

namespace TrekkingForCharity.Api.Tests.Write.CommandValidators
{
    public class UpdateTrekCommandValidatorTests
    {
        [Fact]
        public void Should_BeValid_When_ModelIsCompleteAndNameHasNotChanged()
        {
            var trekId = Guid.NewGuid();

            var validator = new UpdateTrekCommandValidator();
            var command = new UpdateTrekCommand
            {
                Description = "Trek Description",
                Id = trekId
            };
            var result = validator.Validate(command);
            Assert.True(result.IsValid);
            Assert.False(result.Errors.Any());
        }

        [Fact]
        public void Should_BeValid_When_ModelIsCompleteAndNameIsNotInUse()
        {
            var validator = new UpdateTrekCommandValidator();
            var command = new UpdateTrekCommand
            {
                Description = "Trek Description",
                Id = Guid.NewGuid()
            };
            var result = validator.Validate(command);
            Assert.True(result.IsValid);
            Assert.False(result.Errors.Any());
        }

        [Fact]
        public void Should_FailValidation_When_DescriptionIsEmpty()
        {
            var validator = new UpdateTrekCommandValidator();
            var command = new UpdateTrekCommand
            {
                Description = string.Empty
            };
            var result = validator.Validate(command);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, o => o.PropertyName == "Description");
        }

        [Fact]
        public void Should_FailValidation_When_DescriptionIsNull()
        {
            var validator = new UpdateTrekCommandValidator();
            var command = new UpdateTrekCommand();
            var result = validator.Validate(command);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, o => o.PropertyName == "Description");
        }

        [Fact]
        public void Should_FailValidation_When_IdisEmptyGuid()
        {
            var validator = new UpdateTrekCommandValidator();
            var command = new UpdateTrekCommand
            {
                Description = "Trek Description",
                Id = Guid.Empty
            };
            var result = validator.Validate(command);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, o => o.PropertyName == "Id");
        }
    }
}