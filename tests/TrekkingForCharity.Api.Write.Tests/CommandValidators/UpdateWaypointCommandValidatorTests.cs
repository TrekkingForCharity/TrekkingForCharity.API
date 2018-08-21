// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System.Linq;
using TrekkingForCharity.Api.Write.Commands;
using TrekkingForCharity.Api.Write.CommandValidators;
using Xunit;

namespace TrekkingForCharity.Api.Write.Tests.CommandValidators
{
    public class UpdateWaypointCommandValidatorTests
    {
        [Theory]
        [InlineData(23, 128)]
        [InlineData(-23, 128)]
        [InlineData(23, -128)]
        [InlineData(-23, -128)]
        [InlineData(90, 180)]
        [InlineData(-90, -180)]
        public void ShouldNotErrorWhenModelIsValid(double lat, double lng)
        {
            var validator = new UpdateWaypointCommandValidator();
            var command = new UpdateWaypointCommand
            {
                Lat = lat,
                Lng = lng,
                WhenToReach = 1
            };
            var result = validator.Validate(command);
            Assert.True(result.IsValid);
            Assert.False(result.Errors.Any());
        }

        [Theory]
        [InlineData(-181)]
        [InlineData(181)]
        public void ShouldErrorWhenLngIsNoInRange(double lng)
        {
            var validator = new UpdateWaypointCommandValidator();
            var command = new UpdateWaypointCommand
            {
                Lat = 0,
                Lng = lng,
                WhenToReach = 1
            };
            var result = validator.Validate(command);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, o => o.PropertyName == "Lng");
        }

        [Theory]
        [InlineData(-91)]
        [InlineData(91)]
        public void ShouldErrorWhenLatIsNoInRange(double lat)
        {
            var validator = new UpdateWaypointCommandValidator();
            var command = new UpdateWaypointCommand
            {
                Lat = lat,
                Lng = 0,
                WhenToReach = 1
            };
            var result = validator.Validate(command);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, o => o.PropertyName == "Lat");
        }
    }
}