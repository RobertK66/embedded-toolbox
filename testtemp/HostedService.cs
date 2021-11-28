using Microsoft.Extensions.Hosting;


namespace testtemp {
    internal class HostedService : IHostedService {
        List<string> lines = new List<string>();


        public Task StartAsync(CancellationToken cancellationToken) {
            String txt = "StartAsyncAdded in " + GetContext();
            lines.Add(txt);
            Console.WriteLine(txt);
            return Task.CompletedTask;  
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            String txt = "StopAsyncAdded in " + GetContext();
            lines.Add(txt);
            Console.WriteLine(txt);
            return Task.CompletedTask;
        }


        private String GetContext() {
            String retVal = "TaskId:'" + (Task.CurrentId == null ? "<null>" : Task.CurrentId) + "'" ;
            retVal += " Thread[" + Thread.CurrentThread.ManagedThreadId + "]: '" + Thread.CurrentThread.Name??"<none>" + "'";
            retVal += " Proz: " + Thread.GetCurrentProcessorId();
            retVal += " SyncContext: " + (SynchronizationContext.Current?.ToString() ?? "<none>");
            retVal += " TaskScheduler: " + (TaskScheduler.Default?.Id ?? -42);
            return retVal;
        }
    }
}