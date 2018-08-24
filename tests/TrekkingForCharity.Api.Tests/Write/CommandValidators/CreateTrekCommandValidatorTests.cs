// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Slugify;
using TrekkingForCharity.Api.Write.Commands;
using TrekkingForCharity.Api.Write.CommandValidators;
using TrekkingForCharity.Api.Write.Models;
using Xunit;

namespace TrekkingForCharity.Api.Tests.Write.CommandValidators
{
    public class CreateTrekCommandValidatorTests
    {
        [Fact]
        public void Should_FailValidation_When_NameIsNull()
        {
            var trekSlugTable = new Mock<CloudTable>(new Uri("https://test.example.com"));
            trekSlugTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                Result = null
            });
            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));

            var validator = new CreateTrekCommandValidator(trekSlugTable.Object, slugHelper.Object);
            var command = new CreateTrekCommand { Description = "Trek Description" };
            var result = validator.Validate(command);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, o => o.PropertyName == "Name");
        }

        [Fact]
        public void Should_FailValidation_When_NameIsEmpty()
        {
            var trekSlugTable = new Mock<CloudTable>(new Uri("https://test.example.com"));
            trekSlugTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                Result = null
            });
            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));

            var validator = new CreateTrekCommandValidator(trekSlugTable.Object, slugHelper.Object);
            var command = new CreateTrekCommand { Description = "Trek Description", Name = string.Empty };
            var result = validator.Validate(command);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, o => o.PropertyName == "Name");
        }

        [Fact]
        public void Should_FailValidation_When_DescriptionIsNull()
        {
            var trekSlugTable = new Mock<CloudTable>(new Uri("https://test.example.com"));
            trekSlugTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                Result = new TrekSlug('t', "trek-name", $"{Guid.NewGuid()}¬userId")
            });
            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));

            var validator = new CreateTrekCommandValidator(trekSlugTable.Object, slugHelper.Object);
            var command = new CreateTrekCommand { Name = "Trek Name" };
            var result = validator.Validate(command);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, o => o.PropertyName == "Description");
        }

        [Fact]
        public void Should_FailValidation_When_DescriptionIsEmpty()
        {
            var trekSlugTable = new Mock<CloudTable>(new Uri("https://test.example.com"));
            trekSlugTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                Result = new TrekSlug('t', "trek-name", $"{Guid.NewGuid()}¬userId")
            });
            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));

            var validator = new CreateTrekCommandValidator(trekSlugTable.Object, slugHelper.Object);
            var command = new CreateTrekCommand { Name = "Trek Name", Description = string.Empty };
            var result = validator.Validate(command);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, o => o.PropertyName == "Description");
        }

        [Fact]
        public void Should_BeValid_When_ModelIsCompleteAndNameIsNotInUsed()
        {
            var trekSlugTable = new Mock<CloudTable>(new Uri("https://test.example.com"));
            trekSlugTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                Result = null
            });
            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));

            var validator = new CreateTrekCommandValidator(trekSlugTable.Object, slugHelper.Object);
            var command = new CreateTrekCommand
            {
                Name = "Trek Name",
                Description = "Trek Description"
            };
            var result = validator.Validate(command);
            Assert.True(result.IsValid);
            Assert.False(result.Errors.Any());
        }

        [Fact]
        public void Should_FailValidation_WhenNameIsInUse()
        {
            var trekSlugTable = new Mock<CloudTable>(new Uri("https://test.example.com"));
            trekSlugTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                Result = new TrekSlug('t', "trek-name", $"{Guid.NewGuid()}¬userId")
            });
            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));

            var validator = new CreateTrekCommandValidator(trekSlugTable.Object, slugHelper.Object);
            var command = new CreateTrekCommand
            {
                Name = "Trek Name",
                Description = "Trek Description"
            };
            var result = validator.Validate(command);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, o => o.PropertyName == "Name");
        }
    }
}