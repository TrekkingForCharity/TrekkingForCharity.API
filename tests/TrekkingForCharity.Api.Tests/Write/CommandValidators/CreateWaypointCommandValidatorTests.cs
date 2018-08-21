// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System.Linq;
using TrekkingForCharity.Api.Write.Commands;
using TrekkingForCharity.Api.Write.CommandValidators;
using Xunit;

namespace TrekkingForCharity.Api.Tests.Write.CommandValidators
{
    public class CreateWaypointCommandValidatorTests
    {
        [Theory]
        [InlineData("test1", 23, 128)]
        [InlineData("test2", -23, 128)]
        [InlineData("test3", 23, -128)]
        [InlineData("test4", -23, -128)]
        [InlineData("test5", 90, 180)]
        [InlineData("test6", -90, -180)]
        public void ShouldNotErrorWhenModelIsValid(string name, double lat, double lng)
        {
            var validator = new CreateWaypointCommandValidator();
            var command = new CreateWaypointCommand
            {
                Name = name,
                Lat = lat,
                Lng = lng
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
            var validator = new CreateWaypointCommandValidator();
            var command = new CreateWaypointCommand
            {
                Lat = 0,
                Lng = lng
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
            var validator = new CreateWaypointCommandValidator();
            var command = new CreateWaypointCommand
            {
                Lat = lat,
                Lng = 0
            };
            var result = validator.Validate(command);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, o => o.PropertyName == "Lat");
        }
    }
}