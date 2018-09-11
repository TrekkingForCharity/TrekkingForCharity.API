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
using MaybeMonad;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using TrekkingForCharity.Api.Core.Constants;
using TrekkingForCharity.Api.Core.Infrastructure;
using TrekkingForCharity.Api.Write.CommandExecutors;
using TrekkingForCharity.Api.Write.Commands;
using TrekkingForCharity.Api.Write.Models;
using Xunit;

namespace TrekkingForCharity.Api.Tests.Write.CommandExecutors
{
    public class UpdateWaypointCommandExecutorTests
    {
        [Fact]
        public async Task Should_FailToUpdateWaypoint_When_CommandIsInvalid()
        {
            var validator = new Mock<IValidator<UpdateWaypointCommand>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<UpdateWaypointCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("prop", "error")
                }));

            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));

            var waypointTable = new Mock<CloudTable>(new Uri("https://waypoint.example.com"));

            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).ReturnsAsync(() => Maybe.From(new CurrentUser("abc")));

            var
                executor = new UpdateWaypointCommandExecutor(
                    validator.Object,
                    currentUserAccessor.Object,
                    trekTable.Object,
                    waypointTable.Object);

            var cmd = new UpdateWaypointCommand();

            var validationResult = await executor.ValidateAndSetCommand(cmd);
            Assert.False(validationResult.IsValid);

            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsFailure);
            Assert.Equal(ErrorCodes.Validation, executionResult.Error.ErrorCode);
        }

        [Fact]
        public async Task Should_FailToUpdateWaypoint_When_CommandIsNotSet()
        {
            var validator = new Mock<IValidator<UpdateWaypointCommand>>();

            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));

            var waypointTable = new Mock<CloudTable>(new Uri("https://waypoint.example.com"));

            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).ReturnsAsync(() => Maybe.From(new CurrentUser("abc")));

            var
                executor = new UpdateWaypointCommandExecutor(
                    validator.Object,
                    currentUserAccessor.Object,
                    trekTable.Object,
                    waypointTable.Object);

            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsFailure);
            Assert.Equal(ErrorCodes.CommandIsNotSet, executionResult.Error.ErrorCode);
        }

        [Fact]
        public async Task Should_GenerateAuthError_When_NoUserIsPresent()
        {
            var validator = new Mock<IValidator<UpdateWaypointCommand>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<UpdateWaypointCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());

            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));

            var waypointTable = new Mock<CloudTable>(new Uri("https://waypoint.example.com"));

            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).ReturnsAsync(() => Maybe<CurrentUser>.Nothing);

            var
                executor = new UpdateWaypointCommandExecutor(
                    validator.Object,
                    currentUserAccessor.Object,
                    trekTable.Object,
                    waypointTable.Object);

            var cmd = new UpdateWaypointCommand();

            var validationResult = await executor.ValidateAndSetCommand(cmd);
            Assert.True(validationResult.IsValid);
            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsFailure);
            Assert.Equal(ErrorCodes.NotAuthenticated, executionResult.Error.ErrorCode);
        }

        [Fact]
        public void Should_GenerateExceptions_When_ContrustorArgumentsAreNull()
        {
            var ctor = typeof(UpdateWaypointCommandExecutor).GetConstructors().First();
            ctor.TestConstructor();
        }

        [Fact]
        public async Task Should_ReturnAFailedResult_When_WaypointFailsToUpdate()
        {
            var validator = new Mock<IValidator<UpdateWaypointCommand>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<UpdateWaypointCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());

            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));
            trekTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 200,
                Result = new Trek()
            });

            var waypointTable = new Mock<CloudTable>(new Uri("https://waypoint.example.com"));
            waypointTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 200,
                Result = new Waypoint
                {
                    ETag = "*"
                }
            });

            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).ReturnsAsync(() => Maybe.From(new CurrentUser("abc")));

            var
                executor = new UpdateWaypointCommandExecutor(
                    validator.Object,
                    currentUserAccessor.Object,
                    trekTable.Object,
                    waypointTable.Object);

            var cmd = new UpdateWaypointCommand();

            var validationResult = await executor.ValidateAndSetCommand(cmd);
            Assert.True(validationResult.IsValid);
            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsFailure);
            Assert.Equal(ErrorCodes.Creation, executionResult.Error.ErrorCode);
        }

        [Fact]
        public async Task Should_UpdateWaypoint_When_CommandIsValid()
        {
            var validator = new Mock<IValidator<UpdateWaypointCommand>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<UpdateWaypointCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());

            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));
            trekTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 200,
                Result = new Trek()
            });

            var waypointTable = new Mock<CloudTable>(new Uri("https://waypoint.example.com"));
            waypointTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 204,
                Result = new Waypoint
                {
                    ETag = "*"
                }
            });

            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).ReturnsAsync(() => Maybe.From(new CurrentUser("abc")));

            var
                executor = new UpdateWaypointCommandExecutor(
                    validator.Object,
                    currentUserAccessor.Object,
                    trekTable.Object,
                    waypointTable.Object);

            var cmd = new UpdateWaypointCommand();

            var validationResult = await executor.ValidateAndSetCommand(cmd);
            Assert.True(validationResult.IsValid);
            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsSuccess);
        }

        [Fact]
        public async Task Should_FailToUpdateWaypoint_When_TrekIsNotFound()
        {
            var validator = new Mock<IValidator<UpdateWaypointCommand>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<UpdateWaypointCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());

            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));
            trekTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 404
            });

            var waypointTable = new Mock<CloudTable>(new Uri("https://waypoint.example.com"));

            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).ReturnsAsync(() => Maybe.From(new CurrentUser("abc")));

            var
                executor = new UpdateWaypointCommandExecutor(
                    validator.Object,
                    currentUserAccessor.Object,
                    trekTable.Object,
                    waypointTable.Object);

            var cmd = new UpdateWaypointCommand();

            var validationResult = await executor.ValidateAndSetCommand(cmd);
            Assert.True(validationResult.IsValid);
            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsFailure);
            Assert.Equal(ErrorCodes.TrekNotFound, executionResult.Error.ErrorCode);
        }

        [Fact]
        public async Task Should_FailToUpdateWaypoint_When_WaypointIsNotFound()
        {
            var validator = new Mock<IValidator<UpdateWaypointCommand>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<UpdateWaypointCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());

            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));
            trekTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 204,
                Result = new Trek()
            });

            var waypointTable = new Mock<CloudTable>(new Uri("https://waypoint.example.com"));
            waypointTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 404
            });

            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).ReturnsAsync(() => Maybe.From(new CurrentUser("abc")));

            var
                executor = new UpdateWaypointCommandExecutor(
                    validator.Object,
                    currentUserAccessor.Object,
                    trekTable.Object,
                    waypointTable.Object);

            var cmd = new UpdateWaypointCommand();

            var validationResult = await executor.ValidateAndSetCommand(cmd);
            Assert.True(validationResult.IsValid);
            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsFailure);
            Assert.Equal(ErrorCodes.WaypointNotFound, executionResult.Error.ErrorCode);
        }
    }
}