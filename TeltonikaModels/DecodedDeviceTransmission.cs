using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeltonikaModels
{
    public class DecodedDeviceTransmission
    {
        public long Id { get; set; }
        public string DeviceIMEI { get; set; }
        public string Priority { get; set; }
        public DateTime Created { get; set; }
        public DateTime MessageCreated { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        public int Altitude { get; set; }
        public int Angle { get; set; }
        public byte Satellites { get; set; }
        public int Speed { get; set; }
    }
}
