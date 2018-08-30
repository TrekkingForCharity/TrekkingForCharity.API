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

                Assert.Equal(((ArgumentNullException) exception.InnerException).ParamName, ctorParams[i].Name);
            }
        }
    }
}