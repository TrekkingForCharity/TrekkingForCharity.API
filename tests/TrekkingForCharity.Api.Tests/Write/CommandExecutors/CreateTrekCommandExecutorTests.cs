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
using Slugify;
using TrekkingForCharity.Api.Core.Constants;
using TrekkingForCharity.Api.Core.Infrastructure;
using TrekkingForCharity.Api.Write.CommandExecutors;
using TrekkingForCharity.Api.Write.Commands;
using TrekkingForCharity.Api.Write.Models;
using Xunit;

namespace TrekkingForCharity.Api.Tests.Write.CommandExecutors
{
    public class CreateTrekCommandExecutorTests
    {
        [Fact]
        public async Task Should_CreateTrek_When_CommandIsValid()
        {
            var validator = new Mock<IValidator<CreateTrekCommand>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<CreateTrekCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());
            var trekSlugTable = new Mock<CloudTable>(new Uri("https://trekslug.example.com"));
            trekSlugTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 204
            });

            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));
            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));
            trekTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 204
            });
            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).Returns(Maybe.From(new CurrentUser("abc")));

            var
                executor = new CreateTrekCommandExecutor(validator.Object, trekSlugTable.Object, slugHelper.Object,
                    trekTable.Object, currentUserAccessor.Object);

            var cmd = new CreateTrekCommand();

            var validationResult = await executor.ValidateAndSetCommand(cmd);
            Assert.True(validationResult.IsValid);
            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsSuccess);
        }

        [Fact]
        public async Task Should_FailToCreateTrek_When_CommandIsInvalid()
        {
            var validator = new Mock<IValidator<CreateTrekCommand>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<CreateTrekCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("prop", "error")
                }));
            var trekSlugTable = new Mock<CloudTable>(new Uri("https://trekslug.example.com"));

            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));
            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));

            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).Returns(Maybe.From(new CurrentUser("abc")));

            var
                executor = new CreateTrekCommandExecutor(validator.Object, trekSlugTable.Object, slugHelper.Object,
                    trekTable.Object, currentUserAccessor.Object);

            var cmd = new CreateTrekCommand();

            var validationResult = await executor.ValidateAndSetCommand(cmd);
            Assert.False(validationResult.IsValid);

            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsFailure);
            Assert.Equal(ErrorCodes.Validation, executionResult.Error.ErrorCode);
        }

        [Fact]
        public async Task Should_FailToCreateTrek_When_CommandIsNotSet()
        {
            var validator = new Mock<IValidator<CreateTrekCommand>>();

            var trekSlugTable = new Mock<CloudTable>(new Uri("https://trekslug.example.com"));

            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));
            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));

            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).Returns(Maybe.From(new CurrentUser("abc")));

            var
                executor = new CreateTrekCommandExecutor(validator.Object, trekSlugTable.Object, slugHelper.Object,
                    trekTable.Object, currentUserAccessor.Object);

            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsFailure);
            Assert.Equal(ErrorCodes.CommandIsNotSet, executionResult.Error.ErrorCode);
        }

        [Fact]
        public async Task Should_FailToCreateTrek_When_NameIsInUse()
        {
            var validator = new Mock<IValidator<CreateTrekCommand>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<CreateTrekCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());
            var trekSlugTable = new Mock<CloudTable>(new Uri("https://trekslug.example.com"));
            trekSlugTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 200,
                Result = new TrekSlug()
            });

            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));
            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));

            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).Returns(Maybe.From(new CurrentUser("abc")));

            var
                executor = new CreateTrekCommandExecutor(validator.Object, trekSlugTable.Object, slugHelper.Object,
                    trekTable.Object, currentUserAccessor.Object);

            var cmd = new CreateTrekCommand();

            var validationResult = await executor.ValidateAndSetCommand(cmd);
            Assert.True(validationResult.IsValid);
            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsFailure);
            Assert.Equal(ErrorCodes.TrekNameInUse, executionResult.Error.ErrorCode);
        }

        [Fact]
        public async Task Should_GenerateAuthError_When_NoUserIsPresent()
        {
            var validator = new Mock<IValidator<CreateTrekCommand>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<CreateTrekCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());
            var trekSlugTable = new Mock<CloudTable>(new Uri("https://trekslug.example.com"));

            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));
            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));

            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).Returns(Maybe<CurrentUser>.Nothing);

            var
                executor = new CreateTrekCommandExecutor(validator.Object, trekSlugTable.Object, slugHelper.Object,
                    trekTable.Object, currentUserAccessor.Object);

            var cmd = new CreateTrekCommand();

            var validationResult = await executor.ValidateAndSetCommand(cmd);
            Assert.True(validationResult.IsValid);
            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsFailure);
            Assert.Equal(ErrorCodes.NotAuthenticated, executionResult.Error.ErrorCode);
        }

        [Fact]
        public void Should_GenerateExceptions_When_ContrustorArgumentsAreNull()
        {
            var ctor = typeof(CreateTrekCommandExecutor).GetConstructors().First();
            ctor.TestConstructor();
        }

        [Fact]
        public async Task Should_ReturnAFailedResult_When_TrekFailsToInsert()
        {
            var validator = new Mock<IValidator<CreateTrekCommand>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<CreateTrekCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());
            var trekSlugTable = new Mock<CloudTable>(new Uri("https://trekslug.example.com"));
            trekSlugTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 204
            });

            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));
            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));
            trekTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 200
            });
            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).Returns(Maybe.From(new CurrentUser("abc")));

            var
                executor = new CreateTrekCommandExecutor(validator.Object, trekSlugTable.Object, slugHelper.Object,
                    trekTable.Object, currentUserAccessor.Object);

            var cmd = new CreateTrekCommand();

            var validationResult = await executor.ValidateAndSetCommand(cmd);
            Assert.True(validationResult.IsValid);
            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsFailure);
            Assert.Equal(ErrorCodes.Creation, executionResult.Error.ErrorCode);
        }

        [Fact]
        public async Task Should_ReturnAFailedResult_When_TrekSlugFailsToInsert()
        {
            var validator = new Mock<IValidator<CreateTrekCommand>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<CreateTrekCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());
            var trekSlugTable = new Mock<CloudTable>(new Uri("https://trekslug.example.com"));
            trekSlugTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 200
            });

            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));
            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));
            trekTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 204
            });
            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).Returns(Maybe.From(new CurrentUser("abc")));

            var
                executor = new CreateTrekCommandExecutor(validator.Object, trekSlugTable.Object, slugHelper.Object,
                    trekTable.Object, currentUserAccessor.Object);

            var cmd = new CreateTrekCommand();

            var validationResult = await executor.ValidateAndSetCommand(cmd);
            Assert.True(validationResult.IsValid);
            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsFailure);
            Assert.Equal(ErrorCodes.Creation, executionResult.Error.ErrorCode);
        }
    }
}