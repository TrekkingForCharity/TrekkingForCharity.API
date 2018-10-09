// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using TrekkingForCharity.Api.Core.Constants;
using TrekkingForCharity.Api.Read.Models;
using TrekkingForCharity.Api.Read.Queries;
using TrekkingForCharity.Api.Read.QueryProcessors;
using Xunit;

namespace TrekkingForCharity.Api.Tests.Read.QueryProcessors
{
    public class GetTrekByUserAndIdQueryProcessorTests
    {
        [Fact]
        public async Task Should_FailRetrieveTrek_When_QueryIsInValid()
        {
            var validator = new Mock<IValidator<GetTrekByUserAndIdQuery>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<GetTrekByUserAndIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("prop", "error")
                }));
            var trekTable = new Mock<CloudTable>(new Uri("https://treks.example.com"));

            var processor = new GetTrekByUserAndIdQueryProcessor(validator.Object, trekTable.Object);

            var query = new GetTrekByUserAndIdQuery();

            var validationResult = await processor.ValidateAndSetQuery(query);

            Assert.False(validationResult.IsValid);
            Assert.NotEmpty(validationResult.Errors);
        }

        [Fact]
        public async Task Should_FailToRetrieveTrek_When_TrekDoesNotExist()
        {
            var validator = new Mock<IValidator<GetTrekByUserAndIdQuery>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<GetTrekByUserAndIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());
            var trekTable = new Mock<CloudTable>(new Uri("https://treks.example.com"));
            trekTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 200
            });
            var processor = new GetTrekByUserAndIdQueryProcessor(validator.Object, trekTable.Object);

            var query = new GetTrekByUserAndIdQuery
            {
                TrekId = Guid.NewGuid(),
                UserId = new string('*', 10)
            };

            var validationResult = await processor.ValidateAndSetQuery(query);
            var result = await processor.Process();

            Assert.True(validationResult.IsValid);
            Assert.Empty(validationResult.Errors);
            Assert.True(result.IsFailure);
            Assert.Equal(ErrorCodes.TrekNotFound, result.Error.ErrorCode);
        }

        [Fact]
        public void Should_GenerateExceptions_When_ContrustorArgumentsAreNull()
        {
            var ctor = typeof(GetTrekByUserAndIdQueryProcessor).GetConstructors().First();
            ctor.TestConstructor();
        }

        [Fact]
        public async Task Should_RetrieveTrek_When_QueryIsValid()
        {
            var validator = new Mock<IValidator<GetTrekByUserAndIdQuery>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<GetTrekByUserAndIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());
            var trekTable = new Mock<CloudTable>(new Uri("https://treks.example.com"));
            trekTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 204,
                Result = new Trek
                {
                    ETag = "*"
                }
            });
            var processor = new GetTrekByUserAndIdQueryProcessor(validator.Object, trekTable.Object);

            var query = new GetTrekByUserAndIdQuery
            {
                TrekId = Guid.NewGuid(),
                UserId = new string('*', 10)
            };

            var validationResult = await processor.ValidateAndSetQuery(query);
            var result = await processor.Process();

            Assert.True(validationResult.IsValid);
            Assert.Empty(validationResult.Errors);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Trek);
        }
    }
}