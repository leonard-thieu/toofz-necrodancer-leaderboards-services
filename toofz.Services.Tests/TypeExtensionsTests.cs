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
                var name = type.GetSimpleFullName();

                // Assert
                Assert.Equal("System.Collections.Generic.List`1", name);
            }
        }
    }
}
