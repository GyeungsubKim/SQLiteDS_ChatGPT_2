namespace SQLiteDS_ChatGPT_2.Core
{
    public struct Tick
    {
        public int SymbolId;
        public long Time;     // Unix microseconds
        public int Price;     // price * 100
        public int Volume;
        public int Ask;
        public int Bid;
        public int Type;      // 0=Trade,1=Bid,2=Ask
    }
}
