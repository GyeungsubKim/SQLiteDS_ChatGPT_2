using System.Runtime.InteropServices;

namespace SQLiteDS_ChatGPT_2.Core
{
    public static class TickBinarySeralizer
    {
        public static byte[] Serialize(Tick[] ticks)
        {
            int size = ticks.Length * Marshal.SizeOf<Tick>();
            byte[] buffer = new byte[size];

            var span = MemoryMarshal.Cast<byte, Tick>(buffer);
            ticks.CopyTo(span);

            return buffer;
        }
    }
}
