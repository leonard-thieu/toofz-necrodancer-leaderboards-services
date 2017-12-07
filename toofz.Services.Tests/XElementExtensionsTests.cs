using System;
using System.Xml.Linq;
using Xunit;

namespace toofz.Services.Tests
{
    public class XElementExtensionsTests
    {
        public class IsNilMethod
        {
            [DisplayFact(nameof(ArgumentNullException))]
            public void ElIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                XElement el = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    el.IsNil();
                });
            }

            [DisplayFact]
            public void ElDoesNotHaveNilAttribute_ReturnsFalse()
            {
                // Arrange
                var el = new XElement("myElement");

                // Act
                var isNil = el.IsNil();

                // Assert
                Assert.False(isNil);
            }

            [DisplayFact]
            public void ElHasNilAttributeSetToFalse_ReturnsFalse()
            {
                // Arrange
                var el = new XElement(
                    "myElement",
                        new XAttribute(XElementExtensions.Nil, false));

                // Act
                var isNil = el.IsNil();

                // Assert
                Assert.False(isNil);
            }

            [DisplayFact]
            public void ElHasNilAttributeSetToTrue_ReturnsTrue()
            {
                // Arrange
                var el = new XElement(
                    "myElement",
                        new XAttribute(XElementExtensions.Nil, true));

                // Act
                var isNil = el.IsNil();

                // Assert
                Assert.True(isNil);
            }
        }
    }
}
