namespace SQLiteDS_ChatGPT_2.Core
{
    public enum WorkType
    {
        [TableInfo("tblFutCodes", typeof(FutureCode))] FutureCode = 0,
        [TableInfo("tblOptCodes", typeof(OptionCode))] OptionCode = 1,
        [TableInfo("tblHighLow", typeof(HighLow))] HighLow = 2,
        [TableInfo("tblTrd", typeof(Trading))] Trade = 3,
        [TableInfo("tblMst", typeof(Master))] Master = 4,
        [TableInfo("tblFutCur", typeof(FutureCur))] FutureCur = 5,
        [TableInfo("tblOptCur", typeof(OptionCur))] OptionCur = 6,
        [TableInfo("tblFutBid", typeof(BidsAsks))] BidsAsks = 7,
        [TableInfo("tblKsp", typeof(KSP))] Ksp = 8,
        //[TableInfo("tblWork", typeof(WorkLog))] Works = 9
    }
}
