using System.IO;

namespace SQLiteDS_ChatGPT_2.Core
{
    public class UiPacket : IBinarySerializable
    {
        public WorkType Type { get; init; }
        IBinarySerializable Model { get; init; } = default!;
        public long Seq { get; init; }
        public long Time { get; init; }
        public string? Index { get; init; }

        public byte[]? Payload;

        public void WriteTo(BinaryWriter w)
        {
            w.Write((int)Type);
            w.Write(Seq);
            w.Write(Time);

            if (Payload == null)
            {
                w.Write(0);
                return;
            }

            w.Write(Payload.Length);
            w.Write(Payload);
        }
        public static UiPacket ReadFrom(BinaryReader r)
        {
            UiPacket p = new UiPacket
            {
                Type = (WorkType)r.ReadInt32(),
                Seq = r.ReadInt64(),
                Time = r.ReadInt64(),
                Index = r.ReadString(),
            };

            int len = r.ReadInt32();

            if (len > 0)
                p.Payload = r.ReadBytes(len);

            return p;
        }
    }
}
