// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public class GetUpdatesForTrekQueryProcessorTests
    {
        [Fact]
        public async Task Should_FailToRetrieveUpdates_When_NoneAreStored()
        {
            var ctor = typeof(TableQuerySegment<DynamicTableEntity>)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(c => c.GetParameters().Count() == 1);

            var mockQuerySegment =
                ctor.Invoke(new object[]
                {
                    new List<DynamicTableEntity>()
                }) as TableQuerySegment<DynamicTableEntity>;

            var validator = new Mock<IValidator<GetUpdatesForTrekQuery>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<GetUpdatesForTrekQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());
            var waypointTable = new Mock<CloudTable>(new Uri("https://update.example.com"));
            waypointTable
                .Setup(x => x.ExecuteQuerySegmentedAsync(
                    It.IsAny<TableQuery<DynamicTableEntity>>(),
                    It.IsAny<TableContinuationToken>())).ReturnsAsync(() => mockQuerySegment);
            var processor = new GetUpdatesForTrekQueryProcessor(validator.Object, waypointTable.Object);

            var query = new GetUpdatesForTrekQuery
            {
                TrekId = Guid.NewGuid()
            };

            var validationResult = await processor.ValidateAndSetQuery(query);
            var result = await processor.Process();

            Assert.True(validationResult.IsValid);
            Assert.Empty(validationResult.Errors);
            Assert.True(result.IsFailure);
            Assert.Equal(ErrorCodes.UpdatesNotFound, result.Error.ErrorCode);
        }

        [Fact]
        public async Task Should_FailToRetrieveUpdates_When_QueryIsInvalid()
        {
            var ctor = typeof(TableQuerySegment<Update>)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(c => c.GetParameters().Count() == 1);

            var mockQuerySegment =
                ctor.Invoke(new object[]
                {
                    new List<Update>
                    {
                        new Update()
                    }
                }) as TableQuerySegment<Update>;

            var validator = new Mock<IValidator<GetUpdatesForTrekQuery>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<GetUpdatesForTrekQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("prop", "err")
                }));
            var waypointTable = new Mock<CloudTable>(new Uri("https://update.example.com"));
            waypointTable
                .Setup(x => x.ExecuteQuerySegmentedAsync(
                    It.IsAny<TableQuery<Update>>(),
                    It.IsAny<TableContinuationToken>())).ReturnsAsync(() => mockQuerySegment);
            var processor = new GetUpdatesForTrekQueryProcessor(validator.Object, waypointTable.Object);

            var query = new GetUpdatesForTrekQuery
            {
                TrekId = Guid.NewGuid()
            };

            var validationResult = await processor.ValidateAndSetQuery(query);
            Assert.False(validationResult.IsValid);
            Assert.NotEmpty(validationResult.Errors);
        }

        [Fact]
        public void Should_GenerateExceptions_When_ContrustorArgumentsAreNull()
        {
            var ctor = typeof(GetUpdatesForTrekQueryProcessor).GetConstructors().First();
            ctor.TestConstructor();
        }

        [Fact]
        public async Task Should_RetrieveUpdates_When_QueryIsValid()
        {
            var ctor = typeof(TableQuerySegment<Update>)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(c => c.GetParameters().Count() == 1);

            var mockQuerySegment =
                ctor.Invoke(new object[]
                {
                    new List<Update>
                    {
                        new Update()
                    }
                }) as TableQuerySegment<Update>;

            var validator = new Mock<IValidator<GetUpdatesForTrekQuery>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<GetUpdatesForTrekQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());
            var waypointTable = new Mock<CloudTable>(new Uri("https://update.example.com"));
            waypointTable
                .Setup(x => x.ExecuteQuerySegmentedAsync(
                    It.IsAny<TableQuery<Update>>(),
                    It.IsAny<TableContinuationToken>())).ReturnsAsync(() => mockQuerySegment);
            var processor = new GetUpdatesForTrekQueryProcessor(validator.Object, waypointTable.Object);

            var query = new GetUpdatesForTrekQuery
            {
                TrekId = Guid.NewGuid()
            };

            var validationResult = await processor.ValidateAndSetQuery(query);
            var result = await processor.Process();

            Assert.True(validationResult.IsValid);
            Assert.Empty(validationResult.Errors);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Updates);
            Assert.NotEmpty(result.Value.Updates);
        }
    }
}