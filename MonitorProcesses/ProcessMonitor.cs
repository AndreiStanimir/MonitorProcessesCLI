using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorProcesses
{
    public class ProcessMonitor
    {
        public ProcessMonitor(float maximumLifetimeInMinutes, float monitoringFrequencyInMinutes, ILogger logger)
        {
            MaximumLifetimeInMinutes = maximumLifetimeInMinutes;
            MonitoringFrequencyInMinutes = monitoringFrequencyInMinutes;
            this.logger = logger;
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;

        }
        CancellationToken token;
        CancellationTokenSource tokenSource;
        private readonly ILogger logger;

        public float MaximumLifetimeInMinutes { get; }
        public float MonitoringFrequencyInMinutes { get; }

        public async Task<Process?> StartMonitorProcess(int id)
        {
            var process = Process.GetProcessById(id);

            PeriodicTimer periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(MonitoringFrequencyInMinutes));
            float i = 0;
            for (; i < MaximumLifetimeInMinutes
                   && await periodicTimer.WaitForNextTickAsync(token); i += MonitoringFrequencyInMinutes)
            {
                logger.Information(process.TotalProcessorTime.TotalSeconds.ToString());
                if (process.HasExited)
                {
                    logger.Information("process was killed by user");
                    return process;
                }
            };

            if (i >= MaximumLifetimeInMinutes)
            {
                logger.Information($"Killed procces with id:  {id}");
                process.Kill(true);
            }
            return process;
        }
        public async Task<Process?> StartMonitorProcess(string name)
        {
            var processes = Process.GetProcessesByName(name);
            if (processes.Length == 0)
            {
                logger.Error("No process found");
                return null;
            }
            else
            {
                return await StartMonitorProcess(processes[0].Id);
            }
        }
        public async Task Cancel()
        {
            tokenSource.Cancel();
        }
    }

}
