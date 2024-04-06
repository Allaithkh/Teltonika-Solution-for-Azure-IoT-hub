using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TeltonikaCodecsDataConverter;
using TeltonikaCodecsDataConverter.AvlModels;
using TeltonikaModels;

namespace TeltonikaRepository
{
    public class DataCleansingRepository : BackgroundService
    {
        private readonly TeltonikaContext teltonikaContext;
        public DataCleansingRepository(TeltonikaContext teltonikaContext)
        {
            this.teltonikaContext = teltonikaContext;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    DateTime previousDates = DateTime.Now.AddDays(-2);
                    var unprocessedDeviceData =  this.teltonikaContext.DecodedDeviceTransmissions.Where(c => c.Created <= previousDates).ToList();
                    if (unprocessedDeviceData != null && unprocessedDeviceData.Count > 0)
                    {
                        Console.WriteLine($"ProcessTime: {DateTime.Now},PreviousDate: {previousDates}, RecordDeleted: {unprocessedDeviceData.Count}");
                        this.teltonikaContext.RemoveRange(unprocessedDeviceData);
                        await this.teltonikaContext.SaveChangesAsync();
                    }
                    else
                    {
                        Console.WriteLine($"ProcessTime: {DateTime.Now},PreviousDate: {previousDates}, RecordDeleted: 0");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                await Task.Delay(3600000, stoppingToken);
            }
        }
    }
}
