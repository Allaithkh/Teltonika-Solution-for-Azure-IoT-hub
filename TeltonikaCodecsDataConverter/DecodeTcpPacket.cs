using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeltonikaCodecsDataConverter.AvlModels;

namespace TeltonikaCodecsDataConverter
{
    public static class DecodeTcpPacket
    {
        public static TcpDataPacket DecodeTcpPackets(byte[] request)
        {
            var reader = new ReverseBinaryReader(new MemoryStream(request));
            var decoder = new DataDecoder(reader);

            return decoder.DecodeTcpData();
        }
    }
}
