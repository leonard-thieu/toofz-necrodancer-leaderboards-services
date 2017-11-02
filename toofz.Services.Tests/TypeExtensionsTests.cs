using System;
using System.Collections.Generic;
using Xunit;

namespace toofz.Services.Tests
{
    public class TypeExtensionsTests
    {
        public class GetSimpleFullName
        {
            [Fact]
            public void ReturnsSimpleFullName()
            {
                // Arrange
                var type = typeof(List<object>);

                // Act
                var name = TypeExtensions.GetSimpleFullName(type);

                // Assert
                Assert.Equal("System.Collections.Generic.List`1", name);
            }

            [Fact]
            public void TypeIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                Type type = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    TypeExtensions.GetSimpleFullName(type);
                });
            }
        }
    }
}
