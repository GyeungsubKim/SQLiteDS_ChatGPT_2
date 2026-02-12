using System.IO;

namespace SQLiteDS_ChatGPT_2.Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TableInfoAttribute(string tableName, Type modelType) : Attribute
    {
        public string TableName { get; } = tableName;
        public Type ModelType { get; } = modelType;
    }
    public interface IBinarySerializable
    {
        void WriteTo(BinaryWriter writer);
    }
    public class Basic : IBinarySerializable
    {
        public long Idx { get; set; }
        public string? Code { get; set; } = "";
        public long Seq {  get; set; }
        public override string ToString() => $"{Idx} {Code} {Seq}";

        public virtual void WriteTo(BinaryWriter writer)
        {
            writer.Write(Idx);
            writer.Write(Code ?? "");
            writer.Write(Seq);
        }
    }

    //public class WorkLog : IBinarySerializable
    //{
    //    public long Idx { get; set; }
    //    public DateTime Time { get; set; }
    //    public int TableId { get; set; }
    //    public long ForeignKey { get; set; }
    //    public long Seq { get; set; }
    //    public override string ToString() => $"{Idx} {Time} {TableId} {ForeignKey} {Seq}";

    //    public void WriteTo(BinaryWriter writer)
    //    {
    //        writer.Write(Time.ToBinary());
    //        writer.Write(TableId);
    //        writer.Write(ForeignKey);
    //        writer.Write(Seq);
    //    }
    //}

    public class FutureCode : Basic
    {
        public string? Name { get; set; }
        public override string ToString() => $"{base.ToString()} {Name}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Name ?? "");
        }
    }

    public class OptionCode : FutureCode
    {
        public string? Classify { get; set; }
        public string? Month { get; set; }
        public int DoPrice { get; set; }
        public override string ToString() => $"{base.ToString()} {Classify} {Month} {DoPrice:0.00}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Classify ?? "");
            writer.Write(Month ?? "");
            writer.Write(DoPrice!);
        }
    }

    public class KSP : Basic
    {
        public long Time { get; set; }
        public int Price { get; set; }
        public int Change { get; set; }
        public override string ToString() => $"{base.ToString()} {Time} {Price} {Change}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Time);
            writer.Write(Price);
            writer.Write(Change);
        }
    }

    public class Price : Basic
    {
        public int Open { get; set; }
        public int High { get; set; }
        public int Low { get; set; }
        public long Volume { get; set; }
        public int Close { get; set; }
        public long OpenInterest { get; set; }
        public override string ToString() => $"{base.ToString()} {Open} {High} {Low} {Close} {Volume:#,###} {OpenInterest:#,###}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Open);
            writer.Write(High);
            writer.Write(Low);
            writer.Write(Volume);
            writer.Write(Close);
            writer.Write(OpenInterest);
        }
    }

    public class Master : Price
    {
        public long Time { get; set; }
        public int Change { get; set; }
        public long PrevOpenInterest { get; set; }
        public long Turnover { get; set; }
        public long SellQuoteCount { get; set; }
        public long BuyQuoteCount { get; set; }
        public override string ToString() => $"{base.ToString()} {Time} {Change} {PrevOpenInterest} {Turnover} {SellQuoteCount} {BuyQuoteCount}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Time);
            writer.Write(Change);
            writer.Write(PrevOpenInterest);
            writer.Write(Turnover);
            writer.Write(SellQuoteCount);
            writer.Write(BuyQuoteCount);
        }
    }

    public class HighLow : Price
    {
        public long Date { get; set; }
        public override string ToString() => $"{base.ToString()} {Date}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Date);
        }
    }

    public class Current : Price
    {
        public long Time { get; set; }
        public int Change { get; set; }
        public long Turnover { get; set; }
        public long TradeType { get; set; }
        public override string ToString() => $"{base.ToString()} {Time} {Change} {Turnover} {TradeType}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Time);
            writer.Write(Change);
            writer.Write(Turnover);
            writer.Write(TradeType);
        }
    }

    public class FutureCur : Current
    {
        public long SellVolume { get; set; }
        public long BuyVolume { get; set; }
        public int Basis { get; set; }
        public int K200 { get; set; }
        public override string ToString() => $"{base.ToString()} {SellVolume} {BuyVolume} {Basis} {K200}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(SellVolume);
            writer.Write(BuyVolume);
            writer.Write(Basis);
            writer.Write(K200);
        }
    }

    public class BidsAsks : Basic
    {
        public long Time { get; set; }
        public int AskPrice { get; set; }
        public int BidPrice { get; set; }
        public long AskTotalQuantity { get; set; }
        public long BidTotalQuantity { get; set; }
        public byte MarketState { get; set; }
        public long Ask1Quantity { get; set; }
        public long Bid1Quantity { get; set; }
        public override string ToString() => $"{base.ToString()} {Time} {AskPrice} {BidPrice} {AskTotalQuantity} {BidTotalQuantity} {MarketState} {Ask1Quantity} {Bid1Quantity}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Time);
            writer.Write(AskPrice);
            writer.Write(BidPrice);
            writer.Write(AskTotalQuantity);
            writer.Write(BidTotalQuantity);
            writer.Write(MarketState);
            writer.Write(Ask1Quantity);
            writer.Write(Bid1Quantity);
        }
    }

    public class OptionCur : Current
    {
        public int TheoreticalValue { get; set; }
        public int IV { get; set; }
        public int Delta { get; set; }
        public int Gamma { get; set; }
        public int Theta { get; set; }
        public int Vega { get; set; }
        public int Rho { get; set; }
        public int BestAsk { get; set; }
        public int BestBid { get; set; }
        public override string ToString() => $"{base.ToString()} {TheoreticalValue} {IV} {Delta} {Gamma} {Theta} {Vega} {Rho} {BestAsk} {BestBid}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write((int)TheoreticalValue);
            writer.Write((int)IV);
            writer.Write((int)Delta);
            writer.Write((int)Gamma);
            writer.Write((int)Theta);
            writer.Write((int)Vega);
            writer.Write((int)Rho);
            writer.Write((int)BestAsk);
            writer.Write((int)BestBid);
        }
    }

    public class Trading : Basic
    {
        public long Time { get; set; }
        public int Participant { get; set; }
        public long SellVolume { get; set; }
        public long SellValue { get; set; }
        public long BuyVolume { get; set; }
        public long BuyValue { get; set; }
        public override string ToString() => $"{base.ToString()} {Time} {Participant} {SellVolume} {SellValue} {BuyVolume} {BuyValue}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Time);
            writer.Write(Participant);
            writer.Write((long)SellVolume);
            writer.Write((long)SellValue);
            writer.Write((long)BuyVolume);
            writer.Write((long)BuyValue);
        }
    }
}
