using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeltonikaModels
{
    public class DeviceTransmission
    {
        public long Id { get; set; }
        public string DeviceIMEI { get; set; }
        public byte[] DeviceMessage { get; set; }
        public bool IsProcessed { get; set; }
    }
}
