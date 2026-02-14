using System.IO;

namespace SQLiteDS_ChatGPT_2.Core
{
    public static class UiPacketFactory
    {
        public static UiPacket Create(WorkType wt, IBinarySerializable model, long time, long seq)
        {
            using var ms = new MemoryStream(256);
            using var bw = new BinaryWriter(ms);

            model.WriteTo(bw);

            return new UiPacket
            {
                Type = wt,
                Seq = seq,
                Time = time,
                Index = (wt == WorkType.Ksp && model is KSP ksp) ? ksp.Code! : "0",
                Payload = ms.ToArray()
            };
        }
    }
}
