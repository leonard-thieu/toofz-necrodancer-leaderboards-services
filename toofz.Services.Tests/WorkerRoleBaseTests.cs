using System;
using System.Linq;
using System.ServiceProcess;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace toofz.Services.Tests
{
    class WorkerRoleBaseTests
    {
        [TestClass]
        public class Constructor
        {
            [TestMethod]
            public void ServiceNameIsNull_ThrowsArgumentException()
            {
                // Arrange
                string serviceName = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new SimpleWorkerRoleBase(serviceName);
                });
            }

            [TestMethod]
            public void ServiceNameContainsForwardSlash_ThrowsArgumentException()
            {
                // Arrange
                string serviceName = "/";

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new SimpleWorkerRoleBase(serviceName);
                });
            }

            [TestMethod]
            public void ServiceNameContainsBackSlash_ThrowsArgumentException()
            {
                // Arrange
                string serviceName = @"\";

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new SimpleWorkerRoleBase(serviceName);
                });
            }

            [TestMethod]
            public void ServiceNameIsLongerThanMaxNameLength_ThrowsArgumentException()
            {
                // Arrange
                string serviceName = string.Join("", Enumerable.Repeat('a', ServiceBase.MaxNameLength + 1));

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new SimpleWorkerRoleBase(serviceName);
                });
            }
        }
    }
}
