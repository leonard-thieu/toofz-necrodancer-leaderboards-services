using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace toofz.Services.Tests
{
    public class TypeExtensionsTests
    {
        [TestClass]
        public class GetSimpleFullName
        {
            [TestMethod]
            public void ReturnsSimpleFullName()
            {
                // Arrange
                var type = typeof(List<object>);

                // Act
                var name = TypeExtensions.GetSimpleFullName(type);

                // Assert
                Assert.AreEqual("System.Collections.Generic.List`1", name);
            }

            [TestMethod]
            public void TypeIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                Type type = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    TypeExtensions.GetSimpleFullName(type);
                });
            }
        }
    }
}
