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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TeltonikaRepository
{
    public class DecodeDeviceDataRepository : BackgroundService
    {
        private readonly TeltonikaContext teltonikaContext;
        public DecodeDeviceDataRepository(TeltonikaContext teltonikaContext)
        {
            this.teltonikaContext = teltonikaContext;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var unprocessedDeviceData = await this.teltonikaContext.DeviceTransmissions.Where(c => c.IsProcessed == false).ToListAsync();
                    if (unprocessedDeviceData != null && unprocessedDeviceData.Count > 0)
                    {
                        List<DecodedDeviceTransmission> DecodedDeviceTransmissionList = new List<DecodedDeviceTransmission>();
                        foreach (var devicedata in unprocessedDeviceData)
                        {
                            var decodedData = this.DecodeDataPacket(devicedata.DeviceMessage);
                            if (decodedData != null && decodedData.AvlData.Data.Count() > 0)
                            {
                                foreach (var AvlDataRecord in decodedData.AvlData.Data)
                                {
                                    DecodedDeviceTransmission decodedDeviceTransmission = new DecodedDeviceTransmission()
                                    {
                                        DeviceIMEI = devicedata.DeviceIMEI,
                                        Priority = AvlDataRecord.Priority,
                                        MessageCreated = AvlDataRecord.DateTime,
                                        Longitude = (AvlDataRecord.GpsElement.X > 0 ? (AvlDataRecord.GpsElement.X / 10000000) : 0),
                                        Latitude = (AvlDataRecord.GpsElement.Y > 0 ? (AvlDataRecord.GpsElement.Y / 10000000) : 0),
                                        Altitude = AvlDataRecord.GpsElement.Altitude,
                                        Angle = AvlDataRecord.GpsElement.Angle,
                                        Satellites = AvlDataRecord.GpsElement.Satellites,
                                        Speed = AvlDataRecord.GpsElement.Speed,
                                        Created = DateTime.Now,
                                    };
                                    Console.WriteLine($"DeviceIMEI: {decodedDeviceTransmission.DeviceIMEI},MessageCreatedInDevice: {decodedDeviceTransmission.MessageCreated}, MessageDecodedDateTime: {decodedDeviceTransmission.Created}");
                                    DecodedDeviceTransmissionList.Add(decodedDeviceTransmission);
                                }
                            }
                        }

                        if (DecodedDeviceTransmissionList.Any())
                        {
                            this.teltonikaContext.AddRange(DecodedDeviceTransmissionList);
                            this.teltonikaContext.RemoveRange(unprocessedDeviceData);
                            this.teltonikaContext.SaveChanges();
                        }
                        else
                        {
                            this.teltonikaContext.RemoveRange(unprocessedDeviceData);
                            this.teltonikaContext.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                await Task.Delay(30000, stoppingToken);
            }
        }

        private TcpDataPacket DecodeDataPacket(byte[] deviceMessage)
        {
            var fullPacket = new List<byte>();
            int? avlDataLength = null;
            byte[] response;
            var bytes = new byte[4096];
            bytes = deviceMessage;
            int length = bytes.Length;

            fullPacket.AddRange(bytes.Take(length));
            Array.Clear(bytes, 0, bytes.Length);

            var count = fullPacket.Count;

            if (count < 8)
            {
                return null;
            }

            avlDataLength ??= BytesSwapper.Swap(BitConverter.ToInt32([.. fullPacket.GetRange(4, 4)], 0));

            var packetLength = 8 + avlDataLength + 4;
            if (count > packetLength)
            {
                Console.WriteLine("Too much data received.");
                return null;
            }
            if (count != packetLength)
            {
                return null;
            }

            var decodedData = DecodeTcpPacket.DecodeTcpPackets(fullPacket.ToArray());

            avlDataLength = null;
            fullPacket.Clear();
            return decodedData;
        }
    }
}
