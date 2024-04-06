using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeltonikaRepository;

namespace TeltonikaTelemetricTrove
{
    public static class Program
    {
        public static IConfiguration Configuration { get; set; }

        public static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("TELTONIKA_ENVIRONMENT")}.json", true, true)
                .AddEnvironmentVariables();

            var webAssembler = WebApplication.CreateBuilder(
                new WebApplicationOptions
                {
                    EnvironmentName = Environment.GetEnvironmentVariable("TELTONIKA_ENVIRONMENT")
                ,
                    Args = args
                });

            Configuration = webAssembler.Configuration;

            var hostbuilder = new HostBuilder()
                 .UseEnvironment(Environment.GetEnvironmentVariable("TELTONIKA_ENVIRONMENT"))
                 .ConfigureHostConfiguration(c =>
                 {
                     c.AddConfiguration(Configuration);
                 })
                 .UseConsoleLifetime();

            hostbuilder.ConfigureServices((servicecontext, services) =>
            {
                services.AddSingleton(provider => Configuration);
                services.AddHostedService<DecodeDeviceDataRepository>();
                services.AddDbContext<TeltonikaContext>(
                                       opt => opt.UseSqlServer($"{Program.Configuration["DbConnectionString"]}", o => o.CommandTimeout(180))
                                                 .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking), ServiceLifetime.Transient);
            });

            var host = hostbuilder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}