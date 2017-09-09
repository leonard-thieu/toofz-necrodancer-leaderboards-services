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
                ISettings settings = new SimpleSettings();

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new SimpleWorkerRoleBase(serviceName, settings);
                });
            }

            [TestMethod]
            public void ServiceNameIsLongerThanMaxNameLength_ThrowsArgumentException()
            {
                // Arrange
                string serviceName = string.Join("", Enumerable.Repeat('a', ServiceBase.MaxNameLength + 1));
                ISettings settings = new SimpleSettings();

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new SimpleWorkerRoleBase(serviceName, settings);
                });
            }

            [TestMethod]
            public void ServiceNameContainsForwardSlash_ThrowsArgumentException()
            {
                // Arrange
                string serviceName = "/";
                ISettings settings = new SimpleSettings();

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new SimpleWorkerRoleBase(serviceName, settings);
                });
            }

            [TestMethod]
            public void ServiceNameContainsBackSlash_ThrowsArgumentException()
            {
                // Arrange
                string serviceName = @"\";
                ISettings settings = new SimpleSettings();

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new SimpleWorkerRoleBase(serviceName, settings);
                });
            }

            [TestMethod]
            public void SettingsIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string serviceName = "myServiceName";
                ISettings settings = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    new SimpleWorkerRoleBase(serviceName, settings);
                });
            }

            [TestMethod]
            public void ReturnsInstance()
            {
                // Arrange
                string serviceName = "myServiceName";
                ISettings settings = new SimpleSettings();

                // Act
                var worker = new SimpleWorkerRoleBase(serviceName, settings);

                // Assert
                Assert.IsInstanceOfType(worker, typeof(WorkerRoleBase<ISettings>));
            }
        }

        [TestClass]
        public class SettingsProperty
        {
            [TestMethod]
            public void ReturnsInstance()
            {
                // Arrange
                string serviceName = "myServiceName";
                ISettings settings = new SimpleSettings();
                var worker = new SimpleWorkerRoleBase(serviceName, settings);

                // Act
                var settings2 = worker.PublicSettings;

                // Assert
                Assert.IsInstanceOfType(settings2, typeof(ISettings));
            }
        }
    }
}
