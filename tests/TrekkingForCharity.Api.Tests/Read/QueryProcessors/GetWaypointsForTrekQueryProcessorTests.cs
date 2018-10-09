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
    public class GetWaypointsForTrekQueryProcessorTests
    {
        [Fact]
        public async Task Should_FailToRetrieveWaypoints_When_NoneAreStored()
        {
            var ctor = typeof(TableQuerySegment<DynamicTableEntity>)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(c => c.GetParameters().Count() == 1);

            var mockQuerySegment =
                ctor.Invoke(new object[] { new List<DynamicTableEntity>() }) as TableQuerySegment<DynamicTableEntity>;

            var validator = new Mock<IValidator<GetWaypointsForTrekQuery>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<GetWaypointsForTrekQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());
            var waypointTable = new Mock<CloudTable>(new Uri("https://waypoint.example.com"));
            waypointTable
                .Setup(x => x.ExecuteQuerySegmentedAsync(
                    It.IsAny<TableQuery<DynamicTableEntity>>(),
                    It.IsAny<TableContinuationToken>())).ReturnsAsync(() => mockQuerySegment);
            var processor = new GetWaypointsForTrekQueryProcessor(validator.Object, waypointTable.Object);

            var query = new GetWaypointsForTrekQuery
            {
                TrekId = Guid.NewGuid()
            };

            var validationResult = await processor.ValidateAndSetQuery(query);
            var result = await processor.Process();

            Assert.True(validationResult.IsValid);
            Assert.Empty(validationResult.Errors);
            Assert.True(result.IsFailure);
            Assert.Equal(ErrorCodes.WaypointNotFound, result.Error.ErrorCode);
        }

        [Fact]
        public async Task Should_FailToRetrieveWaypoints_When_QueryIsInvalid()
        {
            var ctor = typeof(TableQuerySegment<Waypoint>)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(c => c.GetParameters().Count() == 1);

            var mockQuerySegment =
                ctor.Invoke(new object[]
                {
                    new List<Waypoint>
                    {
                        new Waypoint()
                    }
                }) as TableQuerySegment<Waypoint>;

            var validator = new Mock<IValidator<GetWaypointsForTrekQuery>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<GetWaypointsForTrekQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("prop", "error")
                }));
            var waypointTable = new Mock<CloudTable>(new Uri("https://waypoint.example.com"));
            waypointTable
                .Setup(x => x.ExecuteQuerySegmentedAsync(
                    It.IsAny<TableQuery<Waypoint>>(),
                    It.IsAny<TableContinuationToken>())).ReturnsAsync(() => mockQuerySegment);
            var processor = new GetWaypointsForTrekQueryProcessor(validator.Object, waypointTable.Object);

            var query = new GetWaypointsForTrekQuery
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
            var ctor = typeof(GetWaypointsForTrekQueryProcessor).GetConstructors().First();
            ctor.TestConstructor();
        }

        [Fact]
        public async Task Should_RetrieveWaypoints_When_QueryIsValid()
        {
            var ctor = typeof(TableQuerySegment<Waypoint>)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(c => c.GetParameters().Count() == 1);

            var mockQuerySegment =
                ctor.Invoke(new object[]
                {
                    new List<Waypoint>
                    {
                        new Waypoint()
                    }
                }) as TableQuerySegment<Waypoint>;

            var validator = new Mock<IValidator<GetWaypointsForTrekQuery>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<GetWaypointsForTrekQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());
            var waypointTable = new Mock<CloudTable>(new Uri("https://waypoint.example.com"));
            waypointTable
                .Setup(x => x.ExecuteQuerySegmentedAsync(
                    It.IsAny<TableQuery<Waypoint>>(),
                    It.IsAny<TableContinuationToken>())).ReturnsAsync(() => mockQuerySegment);
            var processor = new GetWaypointsForTrekQueryProcessor(validator.Object, waypointTable.Object);

            var query = new GetWaypointsForTrekQuery
            {
                TrekId = Guid.NewGuid()
            };

            var validationResult = await processor.ValidateAndSetQuery(query);
            var result = await processor.Process();

            Assert.True(validationResult.IsValid);
            Assert.Empty(validationResult.Errors);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Waypoints);
            Assert.NotEmpty(result.Value.Waypoints);
        }
    }
}