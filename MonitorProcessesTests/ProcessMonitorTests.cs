using MonitorProcesses;
using NUnit.Framework;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonitorProcesses
{
    [TestFixture]
    public class ProcessMonitorTests
    {
        //private MockRepository mockRepository;

        private ILogger logger;

        [SetUp]
        public void SetUp()
        {
            //this.mockRepository = new MockRepository(MockBehavior.Strict);
            logger = new LoggerConfiguration()
                .WriteTo.Console(Serilog.Events.LogEventLevel.Information)
                .WriteTo.File("log.txt")
                .CreateLogger();
        }

        private ProcessMonitor CreateProcessMonitor()
        {
            return new ProcessMonitor(1.4f, 0.1f, logger);
        }

        //[TestCase("chrome")]
        //[TestCase("flux")]
        [TestCase("spotify")]

        public async Task MonitorProcess_StateUnderTest_ExpectedBehavior(string processName)
        {
            if (Process.GetProcessesByName(processName).Length == 0)
                Assert.Ignore($"process {processName} does not exist");

            // Arrange
            var processMonitor = this.CreateProcessMonitor();

            // Act
            Process? process = await processMonitor.StartMonitorProcess(processName);
            // Assert
            Assert.NotNull(process);
            Assert.True(process.HasExited);
            //this.mockRepository.VerifyAll();
        }
        [TestCase("chrome.exe")]
        [TestCase("Google Chrome")]
        public async Task NoProcessFound(string processName)
        {
            Process? process = await CreateProcessMonitor().StartMonitorProcess("asdfasdf");
            Assert.IsNull(process);
        }
    }
}
