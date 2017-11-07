using System;
using System.IO;
using System.ServiceProcess;
using Moq;
using Xunit;

namespace toofz.Services.Tests
{
    public class ServiceApplicationTests
    {
        public ServiceApplicationTests()
        {
            worker = mockWorker.Object;
            serviceBase = mockServiceBase.Object;
        }

        private Mock<ServiceBase> mockWorker = new Mock<ServiceBase>();
        private ServiceBase worker;
        private Mock<IServiceBaseStatic> mockServiceBase = new Mock<IServiceBaseStatic>();
        private IServiceBaseStatic serviceBase;

        public class Constructor : ServiceApplicationTests
        {
            [Fact]
            public void ReturnsInstance()
            {
                // Arrange -> Act
                var app = new ServiceApplication<ISettings>(worker, serviceBase);

                // Assert
                Assert.IsAssignableFrom<ServiceApplication<ISettings>>(app);
            }
        }

        public class RunOverrideMethod : ServiceApplicationTests, IDisposable
        {
            public RunOverrideMethod()
            {
                currentDirectory = Directory.GetCurrentDirectory();
                app = new ServiceApplication<ISettings>(worker, serviceBase);
            }

            private readonly string currentDirectory;
            private ServiceApplication<ISettings> app;

            public void Dispose()
            {
                Directory.SetCurrentDirectory(currentDirectory);
            }

            [Fact]
            public void CallsRun()
            {
                // Arrange
                var args = new string[0];
                ISettings settings = new StubSettings();

                // Act
                app.RunOverride(args, settings);

                // Assert
                mockServiceBase.Verify(s => s.Run(worker));
            }
        }
    }
}
