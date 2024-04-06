using System;

namespace TeltonikaCodecsDataConverter.AvlModels.Codec7
{
    [Flags]
    public enum GlobalMaskCodec7 : byte
    {
        GpsElement = 1 << 0,
        IoInt8 = 1 << 1,
        IoInt16 = 1 << 2,
        IoInt32 = 1 << 3,
    }
}
