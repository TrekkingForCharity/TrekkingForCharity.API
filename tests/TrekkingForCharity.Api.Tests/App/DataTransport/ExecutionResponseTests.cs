// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using TrekkingForCharity.Api.App.DataTransport;
using TrekkingForCharity.Api.Core.Constants;
using Xunit;

namespace TrekkingForCharity.Api.Tests.App.DataTransport
{
    public class ExecutionResponseTests
    {
        [Fact]
        public void ShouldHaveSimpleErrorWhenCreatingFailedExecutionResult()
        {
            var er = ExecutionResponse.CreateFailedExecutionResponse((ErrorCodes)1, "Some Error");
            Assert.False(er.Success);
            Assert.Equal((ErrorCodes)1, er.ErrorCode);
            Assert.Equal("Some Error", er.FailMessage);
            Assert.Equal(0, er.Errors.Count);
            Assert.Null(er.Result);
        }

        [Fact]
        public void ShouldHaveSuccessWithDataWhenCreatingSuccess()
        {
            var data = new { d = 123 };
            var er = ExecutionResponse.CreateSuccessfulExecutionResponse(data);
            Assert.True(er.Success);
            Assert.Equal(ErrorCodes.NoError, er.ErrorCode);
            Assert.Equal(string.Empty, er.FailMessage);
            Assert.Equal(0, er.Errors.Count);
            Assert.Equal(data, er.Result);
        }

        [Fact]
        public void ShouldHaveSuccessWithNoDataWhenCreatingEmptySuccess()
        {
            var er = ExecutionResponse.CreateEmptySuccessfulExecutionResponse();
            Assert.True(er.Success);
            Assert.Equal(ErrorCodes.NoError, er.ErrorCode);
            Assert.Equal(string.Empty, er.FailMessage);
            Assert.Equal(0, er.Errors.Count);
            Assert.Null(er.Result);
        }
    }
}
