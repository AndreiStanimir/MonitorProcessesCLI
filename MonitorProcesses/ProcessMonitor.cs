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
        public ProcessMonitor(float maximumLifetimeInMinutes, float monitoringFrequencyInMinutes, ILogger logger, CancellationTokenSource cancellationTokenSource)
        {
            MaximumLifetimeInMinutes = maximumLifetimeInMinutes;
            MonitoringFrequencyInMinutes = monitoringFrequencyInMinutes;
            this.logger = logger;
            tokenSource = cancellationTokenSource;
            token = tokenSource.Token;

        }
        CancellationToken token;
        CancellationTokenSource tokenSource;
        private readonly ILogger logger;

        public float MaximumLifetimeInMinutes { get; }
        public float MonitoringFrequencyInMinutes { get; }

        public async Task<Process?> StartMonitorProcessAsync(Process process)
        {
            PeriodicTimer periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(MonitoringFrequencyInMinutes));
            float i = 0;
            for (; i < MaximumLifetimeInMinutes
                   && await  periodicTimer.WaitForNextTickAsync(token); i += MonitoringFrequencyInMinutes)
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
                logger.Information($"Killed procces with id:  {process.Id}");
                process.Kill(true);
            }
            return process;
        }
        public async Task StartMonitorProcessAsync(string name)
        {
            PeriodicTimer periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(MonitoringFrequencyInMinutes));
            var processes = Process.GetProcessesByName(name);
            while (await periodicTimer.WaitForNextTickAsync(token))
            {
                if (!processes.Any() ||
                    processes.All(p => p.HasExited))
                    processes = Process.GetProcessesByName(name);
                //if (processes.Length == 0)
                //{
                //    logger.Error("No process found");
                //    return null;
                //}

                await Parallel.ForEachAsync(processes, token, async (process, token) =>
                {
                    await StartMonitorProcessAsync(process);
                });
            }
        }
        public async Task Cancel()
        {
            tokenSource.Cancel();
        }
    }

}
