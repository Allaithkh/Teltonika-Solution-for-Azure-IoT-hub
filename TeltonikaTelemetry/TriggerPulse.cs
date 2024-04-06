using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Primitives;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Diagnostics;
using TeltonikaModels;
using TeltonikaRepository;

namespace TeltonikaTelemetry
{
    public class TriggerPulse(
        BlobContainerClient storageContainerClient,
        int maximumBatchSize,
        string subscriberGroup,
        string eventBrokerConnectionString,
        string eventBrokerName,
        EventProcessorOptions clientOptions = default) : PluggableCheckpointStoreEventProcessor<EventProcessorPartition>(
                new BlobCheckpointStore(storageContainerClient),
                maximumBatchSize,
                subscriberGroup,
                eventBrokerConnectionString,
                eventBrokerName,
                clientOptions)
    {
        protected override Task OnProcessingErrorAsync(Exception exception, EventProcessorPartition partition, string operationDescription, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override async Task OnProcessingEventBatchAsync(IEnumerable<EventData> events, EventProcessorPartition partition, CancellationToken cancellationToken)
        {
            EventData lastEvent = null;

            try
            {
                Console.WriteLine($"Partitons Received {partition.PartitionId}");
                using var serviceScope = Program.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
                using var teltonikaContext = serviceScope.ServiceProvider.GetService<TeltonikaContext>();
                foreach (var message in events)
                {
                    try
                    {
                        var bytes = new byte[4096];
                        var deviceIMEI = message.SystemProperties["iothub-connection-device-id"].ToString();
                        bytes = message.EventBody.ToArray();
                        DeviceTransmission deviceTransmission = new ()
                        {
                            DeviceIMEI = deviceIMEI,
                            DeviceMessage = bytes,
                            IsProcessed = false,
                        };
                        teltonikaContext.DeviceTransmissions.Add(deviceTransmission);
                        teltonikaContext.SaveChanges();
                        Console.WriteLine($"DeviceIMEI: {deviceTransmission.DeviceIMEI}, MessageSavedDateTime: {DateTime.Now}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine(message.EventBody.ToString());
                    }

                    lastEvent = message;
                }

                if (lastEvent != null)
                {
                    await this.UpdateCheckpointAsync(
                        partition.PartitionId,
                        lastEvent.Offset,
                        lastEvent.SequenceNumber,
                        cancellationToken)
                    .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }
    }
}
