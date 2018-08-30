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
    public class StartTrekCommandExecutorTests
    {
        [Fact]
        public async Task Should_FailToStartTrek_When_CommandIsInvalid()
        {
            var validator = new Mock<IValidator<StartTrekCommand>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<StartTrekCommand>(), It.IsAny<CancellationToken>()))
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
                executor = new StartTrekCommandExecutor(
                    validator.Object,
                    currentUserAccessor.Object,
                    trekTable.Object
                    );

            var cmd = new StartTrekCommand();

            var validationResult = await executor.ValidateAndSetCommand(cmd);
            Assert.False(validationResult.IsValid);

            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsFailure);
            Assert.Equal(ErrorCodes.Validation, executionResult.Error.ErrorCode);
        }

        [Fact]
        public async Task Should_FailToStartTrek_When_CommandIsNotSet()
        {
            var validator = new Mock<IValidator<StartTrekCommand>>();
            
            var trekSlugTable = new Mock<CloudTable>(new Uri("https://trekslug.example.com"));
            

            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));
            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));
            
            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).Returns(Maybe.From(new CurrentUser("abc")));

            var
                executor = new StartTrekCommandExecutor(
                    validator.Object,
                    currentUserAccessor.Object,
                    trekTable.Object
                );

            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsFailure);
            Assert.Equal(ErrorCodes.CommandIsNotSet, executionResult.Error.ErrorCode);
        }

        [Fact]
        public async Task Should_StartTrek_When_CommandIsValid()
        {
            var validator = new Mock<IValidator<StartTrekCommand>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<StartTrekCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());
            var trekSlugTable = new Mock<CloudTable>(new Uri("https://trekslug.example.com"));
            

            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));
            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));
            trekTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 204,
                Result = new Trek
                {
                    ETag = "*"
                }
            });
            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).Returns(Maybe.From(new CurrentUser("abc")));

            var
                executor = new StartTrekCommandExecutor(
                    validator.Object,
                    currentUserAccessor.Object,
                    trekTable.Object
                );

            var cmd = new StartTrekCommand();

            var validationResult = await executor.ValidateAndSetCommand(cmd);
            Assert.True(validationResult.IsValid);
            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsSuccess);
        }


        [Fact]
        public async Task Should_FailToStartTrek_When_TrekIsNotFound()
        {
            var validator = new Mock<IValidator<StartTrekCommand>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<StartTrekCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());
            var trekSlugTable = new Mock<CloudTable>(new Uri("https://trekslug.example.com"));


            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));
            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));
            trekTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 404
            });
            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).Returns(Maybe.From(new CurrentUser("abc")));

            var
                executor = new StartTrekCommandExecutor(
                    validator.Object,
                    currentUserAccessor.Object,
                    trekTable.Object
                );

            var cmd = new StartTrekCommand();

            var validationResult = await executor.ValidateAndSetCommand(cmd);
            Assert.True(validationResult.IsValid);
            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsFailure);
            Assert.Equal(ErrorCodes.TrekNotFound, executionResult.Error.ErrorCode);
        }

        [Fact]
        public async Task Should_GenerateAuthError_When_NoUserIsPresent()
        {
            var validator = new Mock<IValidator<StartTrekCommand>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<StartTrekCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());
            var trekSlugTable = new Mock<CloudTable>(new Uri("https://trekslug.example.com"));
            
            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));
            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));
            
            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).Returns(Maybe<CurrentUser>.Nothing);

            var
                executor = new StartTrekCommandExecutor(
                    validator.Object,
                    currentUserAccessor.Object,
                    trekTable.Object
                );

            var cmd = new StartTrekCommand();

            var validationResult = await executor.ValidateAndSetCommand(cmd);
            Assert.True(validationResult.IsValid);
            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsFailure);
            Assert.Equal(ErrorCodes.NotAuthenticated, executionResult.Error.ErrorCode);
        }

        

        [Fact]
        public async Task Should_ReturnAFailedResult_When_TrekFailsToUpdate()
        {
            var validator = new Mock<IValidator<StartTrekCommand>>();
            validator.Setup(x => x.ValidateAsync(It.IsAny<StartTrekCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ValidationResult());
            var trekSlugTable = new Mock<CloudTable>(new Uri("https://trekslug.example.com"));
            

            var slugHelper = new Mock<ISlugHelper>();
            slugHelper.Setup(x => x.GenerateSlug(It.IsAny<string>())).Returns(new string('*', 10));
            var trekTable = new Mock<CloudTable>(new Uri("https://trek.example.com"));
            trekTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(() => new TableResult
            {
                HttpStatusCode = 200,
                Result = new Trek
                {
                ETag = "*"
            }
            });
            var currentUserAccessor = new Mock<ICurrentUserAccessor>();
            currentUserAccessor.Setup(x => x.GetCurrentUser()).Returns(Maybe.From(new CurrentUser("abc")));

            var
                executor = new StartTrekCommandExecutor(
                    validator.Object,
                    currentUserAccessor.Object,
                    trekTable.Object
                );

            var cmd = new StartTrekCommand();

            var validationResult = await executor.ValidateAndSetCommand(cmd);
            Assert.True(validationResult.IsValid);
            var executionResult = await executor.Execute();

            Assert.True(executionResult.IsFailure);
            Assert.Equal(ErrorCodes.Creation, executionResult.Error.ErrorCode);
        }
        [Fact]
        public void Should_GenerateExceptions_When_ContrustorArgumentsAreNull()
        {
            var ctor = typeof(StartTrekCommandExecutor).GetConstructors().First();
            ctor.TestConstructor();
        }

    }
}