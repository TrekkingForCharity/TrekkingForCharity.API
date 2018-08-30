// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System;
using System.Collections.Generic;
using System.Reflection;
using Moq;
using Xunit;

namespace TrekkingForCharity.Api.Tests
{
    public static class TestHelpers
    {
        public static dynamic GenerateMock(this Type type)
        {
            switch (type.Name.ToLower())
            {
                case "cloudtable":
                    return Activator.CreateInstance(
                        typeof(Mock<>).MakeGenericType(type),
                        new Uri("http://table.example.com"));
                default:
                    return Activator.CreateInstance(typeof(Mock<>).MakeGenericType(type));
            }
        }

        public static void TestConstructor(this ConstructorInfo constructorInfo)
        {
            var ctorParams = constructorInfo.GetParameters();
            for (var i = 0; i < ctorParams.Length; i++)
            {
                var paramsToPass = new List<object>();
                for (var paramToPassCount = 0; paramToPassCount < ctorParams.Length; paramToPassCount++)
                {
                    if (paramToPassCount < i)
                    {
                        var inst = ctorParams[paramToPassCount].ParameterType.GenerateMock();
                        paramsToPass.Add(inst.Object);
                    }
                    else
                    {
                        paramsToPass.Add(null);
                    }
                }

                var exception =
                    Assert.Throws<TargetInvocationException>(() => constructorInfo.Invoke(paramsToPass.ToArray()));
                Assert.NotNull(exception);
                Assert.NotNull(exception.InnerException);

                Assert.Equal(((ArgumentNullException)exception.InnerException).ParamName, ctorParams[i].Name);
            }
        }
    }
}