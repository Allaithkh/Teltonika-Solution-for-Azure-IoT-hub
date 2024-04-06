using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs.Primitives;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeltonikaRepository;
using TeltonikaTelemetry;

namespace TeltonikaTelemetry
{
    public static class Program
    {
        public static IServiceProvider ServiceProvider { get; set; }
        public static IConfiguration Configuration { get; set; }

        public static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("TELTONIKA_ENVIRONMENT")}.json", true, true)
                .AddEnvironmentVariables();

            var webAssembler = WebApplication.CreateBuilder(
                new WebApplicationOptions 
                { EnvironmentName = Environment.GetEnvironmentVariable("TELTONIKA_ENVIRONMENT")
                , Args = args });

            ServiceProvider = ServiceProviderFactory();
            Configuration = webAssembler.Configuration;

            var hostbuilder = new HostBuilder()
                 .UseEnvironment(Environment.GetEnvironmentVariable("TELTONIKA_ENVIRONMENT"))
                 .ConfigureHostConfiguration(c =>
                 {
                     c.AddConfiguration(Configuration);
                 })
                 .UseConsoleLifetime();

            var eventBrokerName = $"{Configuration["EventBrokerName"]}";
            var eventBrokerConnectionString = $"{Configuration["EventBrokerConnectionString"]}";
            var subscriberGroup = $"{Configuration["EventBrokerSubscriberGroup"]}";

            var blobName = $"{Configuration["BlobName"]}";
            var blobConnectionString = $"{Configuration["BlobConnectionString"]}";
            var storageContainerClient = new BlobContainerClient(
                blobConnectionString,
                blobName);
            await storageContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var processor = new TriggerPulse(
                storageContainerClient,
                256,
                subscriberGroup,
                eventBrokerConnectionString,
                eventBrokerName);


            using var cancellationSource = new CancellationTokenSource();
            try
            {
                await processor.StartProcessingAsync(cancellationSource.Token);
                await Task.Delay(Timeout.Infinite, cancellationSource.Token);
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
            finally
            {
                await processor.StopProcessingAsync();
            }

            var host = hostbuilder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }

        private static IServiceProvider ServiceProviderFactory()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(provider => Configuration);
            services.AddDbContext<TeltonikaContext>(
                                   opt => opt.UseSqlServer($"{Program.Configuration["DbConnectionString"]}", o => o.CommandTimeout(180))
                                             .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking), ServiceLifetime.Transient);
            return services.BuildServiceProvider();
        }
    }
}