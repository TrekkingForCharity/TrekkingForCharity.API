// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System.Linq;
using TrekkingForCharity.Api.Write.DataTransport;
using Xunit;

namespace TrekkingForCharity.Api.Tests.Write.DataTransport
{
    public class ExecutionResultTests
    {
        [Fact]
        public void ShouldHaveErrorsWhenCreatingFailedExecutionResultAgainstProperty()
        {
            var er = ExecutionResult.CreateFailedExecutionResultWithSingleValidationError("Name", "ERR-001",
                "Some Error");
            Assert.False(er.Success);
            Assert.Equal(string.Empty, er.ErrorCode);
            Assert.Equal(string.Empty, er.FailMessage);
            Assert.Equal(1, er.Errors.Count);
            var error = er.Errors.First();
            Assert.Equal("Name", error.Property);
            Assert.Equal("ERR-001", error.ErrorCode);
            Assert.Equal("Some Error", error.Message);
            Assert.Null(er.Result);
        }

        [Fact]
        public void ShouldHaveSimpleErrorWhenCreatingFailedExecutionResult()
        {
            var er = ExecutionResult.CreateFailedExecutionResult("ERR-001", "Some Error");
            Assert.False(er.Success);
            Assert.Equal("ERR-001", er.ErrorCode);
            Assert.Equal("Some Error", er.FailMessage);
            Assert.Equal(0, er.Errors.Count);
            Assert.Null(er.Result);
        }

        [Fact]
        public void ShouldHaveSuccessWithDataWhenCreatingSuccess()
        {
            var data = new { d = 123 };
            var er = ExecutionResult.CreateSuccessfulExecutionResult(data);
            Assert.True(er.Success);
            Assert.Equal(string.Empty, er.ErrorCode);
            Assert.Equal(string.Empty, er.FailMessage);
            Assert.Equal(0, er.Errors.Count);
            Assert.Equal(data, er.Result);
        }

        [Fact]
        public void ShouldHaveSuccessWithNoDataWhenCreatingEmptySuccess()
        {
            var er = ExecutionResult.CreateEmptySuccessfulExecutionResult();
            Assert.True(er.Success);
            Assert.Equal(string.Empty, er.ErrorCode);
            Assert.Equal(string.Empty, er.FailMessage);
            Assert.Equal(0, er.Errors.Count);
            Assert.Null(er.Result);
        }
    }
}