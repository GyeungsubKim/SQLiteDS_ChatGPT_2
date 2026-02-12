using System.Buffers.Binary;

namespace SQLiteDS_ChatGPT_2.Pipe
{
    public static class PipePacket
    {
        public static void WriteLength(Span<byte> buffer, int len)
        {
            BinaryPrimitives.WriteInt32LittleEndian(buffer, len);
        }
    }
}
