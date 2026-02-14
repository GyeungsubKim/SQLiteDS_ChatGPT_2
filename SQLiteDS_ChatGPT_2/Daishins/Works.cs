using CPSYSDIBLib;
using CPUTILLib;
using DSCBO1Lib;
using SQLiteDS_ChatGPT_2.Core;
using SQLiteDS_ChatGPT_2.Views;

namespace SQLiteDS_ChatGPT_2.Daishins
{
    public class Works(ViewModel vm)
    {
        private readonly CybosConnection _connection = new(vm);
        private readonly Daishin _daishin = new(vm);
        private readonly ViewModel _vm = vm;

        public event Action<WorkType, Basic>? OnReceive;
        public List<FutureCode>? FuCodes;
        public List<OptionCode>? OpCodes;

        public async Task Run()
        {
            await Task.Run(() => GetCodes());
            await Task.Run(() => KospiHighLow());
            await Task.Run(() => StartTrade());
            await Task.Run(() => InputServer());
            await Task.Run(() => InsertOption());
        }
        OptionCurOnly? oCur;
        private async Task InsertOption()
        {
            int count = 0;
            try
            {
                _vm.AddLog("옵션 코드 입력 중...");

                if (OpCodes == null || OpCodes.Count == 0)
                {
                    _vm.AddLog("옵션 코드가 없습니다.");
                    return;
                }

                oCur ??= new();
                oCur.Received += OCur_Received;

                int curr = 0;
                int month = int.Parse(OpCodes![0].Month!.ToString());

                // 당월과 익월 별도로 2번 입력한다
                // 콜,풋을 동시에 입력 하는데
                // 한쪽이 200.00을 넘어면 제외한다????
                // 26.1월 만기일 부터 코드 번호가 바뀜
                // 선물, 콜, 풋, 연계 : A, B, C, D
                List<string> Headers = ["A", "B", "C", "D"];
                for (int m = 0; m < 2; m++)
                {
                    // 월물이 같은 것만 가져온다
                    var codeList = OpCodes
                        .Where(x => x.Month == (month + m).ToString() && x.Code!.StartsWith('B'))
                        .ToList();
                    for (int i = 0; i < codeList.Count; i++)
                    {
                        OptionCode codes = codeList[i];
                        if (!int.TryParse(codes.Month!, out int mm)) continue;
                        if (!codes.Code!.StartsWith('B'))
                            continue;
                        for (int c = 1; c < 3; c++)
                        {
                            if (count >= 364) break;
                            string code = Headers[c] + codes.Code[1..];
                            FutureHighLow(code);
                            var price = await MstOption(code, m);

                            if (count < 364)
                            {
                                // 차이는 비교하지 말고 마지막 종가가 가격 범위 외면 제외한다.
                                if (price > 200.00m || price <= 0.01m)
                                    continue;
                                try
                                {
                                    oCur.SetInputValue(0, code);
                                    oCur.SubscribeLatest();
                                }
                                catch (Exception ex)
                                {
                                    _vm.AddLog($"옵션 코드 서버 입력 실패({code}): {ex.Message}");
                                    continue;
                                }
                                count++;
                            }
                        }
                    }
                    string mon = "당";
                    if (m > 0) mon = "당월 + 익";
                    else curr = count;
                    _vm.AddLog($"{mon}월 {count - curr}개 서버 입력");
                }
                _vm.AddLog($"옵션 입력 개수 : {count}");
            }
            catch (Exception ex)
            {
                _vm.AddLog($"서버에 옵션 코드 입력 중 에러 : {ex.Message}\n입력 개수 : {count}개");
            }
            _vm.AddLog("옵션 코드 입력 종료!");
        }
        private async Task<decimal> MstOption(string code, int index)
        {
            decimal result = -1m;
            try
            {
                OptionMst mst = new();
                mst.SetInputValue(0, code);
                mst.BlockRequest();

                Core.OptionCur cur = new()
                {
                    Code = ComHelpers.AsString(mst!.GetHeaderValue(0)),
                    Time = ComHelpers.AsLong(mst!.GetHeaderValue(55)),
                    Close = ComHelpers.AsDecimal(mst!.GetHeaderValue(93)),
                    Change = ComHelpers.AsDecimal(mst!.GetHeaderValue(93)) -
                            ComHelpers.AsDecimal(mst!.GetHeaderValue(27)),
                    Open = ComHelpers.AsDecimal(mst!.GetHeaderValue(94)),
                    High = ComHelpers.AsDecimal(mst!.GetHeaderValue(95)),
                    Low = ComHelpers.AsDecimal(mst!.GetHeaderValue(96)),
                    Volume = ComHelpers.AsLong(mst!.GetHeaderValue(97)),
                    Turnover = ComHelpers.AsLong(mst!.GetHeaderValue(98)),
                    TheoreticalValue = 0,
                    IV = ComHelpers.AsDecimal(mst!.GetHeaderValue(108)),
                    Delta = ComHelpers.AsDecimal(mst!.GetHeaderValue(109)),
                    Gamma = ComHelpers.AsDecimal(mst!.GetHeaderValue(110)),
                    Theta = ComHelpers.AsDecimal(mst!.GetHeaderValue(111)),
                    Vega = ComHelpers.AsDecimal(mst!.GetHeaderValue(112)),
                    Rho = ComHelpers.AsDecimal(mst!.GetHeaderValue(113)),
                    OpenInterest = ComHelpers.AsLong(mst!.GetHeaderValue(99)),
                    BestAsk = ComHelpers.AsDecimal(mst!.GetHeaderValue(58)),
                    BestBid = ComHelpers.AsDecimal(mst!.GetHeaderValue(59)),
                    TradeType = (byte)ComHelpers.AsLong(mst!.GetHeaderValue(118)),
                    Seq = DateTime.Now.Ticks,
                };
                OnReceive!(WorkType.OptionCur, cur);
                await Task.Delay(1);
                result = cur.Close;
            }
            catch (Exception ex)
            {
                _vm.AddLog($"옵션 현재가 받는 중 ㅇ에러 : {ex.Message}");
            }
            return result;
        }
        private async void OCur_Received()
        {
            try
            {
                if (int.TryParse(oCur!.GetHeaderValue(7).ToString(), out int vol) && vol <= 0) return;
                Core.OptionCur cur = new()
                {
                    Code = oCur!.GetHeaderValue(0).ToString(),
                    Time = ComHelpers.AsLong(oCur!.GetHeaderValue(1)),
                    Close = ComHelpers.AsDecimal(oCur!.GetHeaderValue(2)),
                    Change = ComHelpers.AsDecimal(oCur!.GetHeaderValue(3)),
                    Open = ComHelpers.AsDecimal(oCur!.GetHeaderValue(4)),
                    High = ComHelpers.AsDecimal(oCur!.GetHeaderValue(5)),
                    Low = ComHelpers.AsDecimal(oCur!.GetHeaderValue(6)),
                    Volume = ComHelpers.AsLong(oCur!.GetHeaderValue(7)),
                    Turnover = ComHelpers.AsLong(oCur!.GetHeaderValue(8)),
                    TheoreticalValue = ComHelpers.AsDecimal(oCur!.GetHeaderValue(9)),
                    IV = ComHelpers.AsDecimal(oCur!.GetHeaderValue(10)),
                    Delta = ComHelpers.AsDecimal(oCur!.GetHeaderValue(11)),
                    Gamma = ComHelpers.AsDecimal(oCur!.GetHeaderValue(12)),
                    Theta = ComHelpers.AsDecimal(oCur!.GetHeaderValue(13)),
                    Vega = ComHelpers.AsDecimal(oCur!.GetHeaderValue(14)),
                    Rho = ComHelpers.AsDecimal(oCur!.GetHeaderValue(15)),
                    OpenInterest = ComHelpers.AsLong(oCur!.GetHeaderValue(16)),
                    BestAsk = ComHelpers.AsDecimal(oCur!.GetHeaderValue(17)),
                    BestBid = ComHelpers.AsDecimal(oCur!.GetHeaderValue(18)),
                    TradeType = (byte)ComHelpers.AsLong(oCur!.GetHeaderValue(23)),
                    Seq = DateTime.Now.Ticks,
                };
                OnReceive!(WorkType.OptionCur, cur);
                await Task.Delay(1).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _vm.AddLog($"옵션 현재가 받는 중 에러 : {ex.Message}");
            }
        }

        private FutureCurOnly? fCur;       // 선물 현재가
        private FutureJpBid? fBid;       // 선물 호가
        private StockIndexis? sCur;        // 코스피 및 코스닥 지수 10초 단위
        private FutureIndexi? kCur;        // 코스피 200 지수 2초 단위
        private FOExpectCur? fExc;       // 예상지수
        private void InputServer()
        {
            try
            {
                _vm.AddLog("선물 코드 입력 시작");
                if (FuCodes == null || FuCodes.Count == 0)
                {
                    _vm.AddLog("선물 코드가 없습니다.");
                    return;
                }
                string eCode = string.Empty;

                fCur ??= new();
                fCur.Received += FCur_Received;
                fBid ??= new();
                fBid.Received += FBid_Received;

                _vm.AddLog("선물 주간, 현재가 가저오기 및 코드 입력");
                for (int i = 0; i < FuCodes?.Count; i++)
                {
                    string code = FuCodes[i].Code ?? string.Empty;
                    if (string.IsNullOrEmpty(code)) continue;
                    if (eCode == string.Empty && code != "10100")
                        eCode = code;
                    FutureHighLow(code);
                    GetDataMst(code);

                    fCur.SetInputValue(0, code);
                    fCur.SubscribeLatest();

                    fBid.SetInputValue(0, code);
                    fBid.SubscribeLatest();
                }

                fExc ??= new();
                fExc.Received += FExc_Received;
                fExc.SetInputValue(0, eCode);
                fExc.SetInputValue(1, "*");
                fExc.SubscribeLatest();

                StockData();

                sCur ??= new();
                sCur.Received += SCur_Received;

                string[] uCodes = ["U001", "U201"];
                foreach (string code in uCodes)
                {
                    sCur.SetInputValue(0, code);
                    sCur.SubscribeLatest();
                }

                kCur ??= new();
                kCur.Received += KCur_Received;
                kCur.SetInputValue(0, "00800");
                kCur.SubscribeLatest();

                _vm.AddLog("서버에 코드 입력 종료");
            }
            catch (Exception ex)
            {
                _vm.AddLog($"InputServer Error : {ex.Message}");
            }

        }
        private void KCur_Received()
        {
            try
            {
                KSP ksp = new()
                {
                    Code = "3",
                    Time = ComHelpers.AsLong(kCur!.GetHeaderValue(1)),
                    Price = ComHelpers.AsDecimal(kCur!.GetHeaderValue(2)),
                    Change = ComHelpers.AsDecimal(kCur!.GetHeaderValue(3)),
                    Seq = DateTime.Now.Ticks,
                };
                OnReceive!(WorkType.Ksp, ksp);//.InsertKsp(ksp);
            }
            catch (Exception ex)
            {
                _vm.AddLog($"kospi200 지수 받는 중 에러 : {ex.Message}");
            }
        }
        private void SCur_Received()
        {
            try
            {
                string code = sCur!.GetHeaderValue(7).ToString();
                int index = code.Equals("U001", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
                KSP ksp = new()
                {
                    Code = index.ToString(),
                    Time = ComHelpers.AsLong(sCur!.GetHeaderValue(1)),
                    Price = ComHelpers.AsDecimal(sCur!.GetHeaderValue(2)),
                    Change = ComHelpers.AsDecimal(sCur!.GetHeaderValue(3)),
                    Seq = DateTime.Now.Ticks,
                };
                OnReceive!(WorkType.Ksp, ksp);//.InsertKsp(ksp);
            }
            catch (Exception ex)
            {
                _vm.AddLog($"Kospi 지수 받는 중 에러 : {ex.Message}");
            }
        }
        private async void StockData()
        {
            try
            {
                _vm.AddLog("주가 지수 입력 중...");
                StockChart cls = new();
                string[] codes = ["U001", "U201", "U180"];
                int[] ip = [0, 1, 2, 3, 4, 5, 6];
                int index = 0;
                foreach (string code in codes)
                {
                    cls.SetInputValue(0, code);
                    cls.SetInputValue(1, char.Parse("2"));
                    cls.SetInputValue(4, 1);
                    cls.SetInputValue(5, ip);
                    cls.BlockRequest();
                    await _connection!.WaitTime(nameof(StockData));

                    if (int.TryParse(cls.GetDataValue(0, 0).ToString(), out int d) &&
                        d == int.Parse(DateTime.Today.ToString("yyyyMMdd")))
                    {
                        for (int i = 2; i < 6; i++)
                        {
                            int close = ComHelpers.AsDecimal(cls.GetDataValue(i, 0));
                            int past = ComHelpers.AsDecimal(cls.GetDataValue(5, 0)) -
                                       ComHelpers.AsDecimal(cls.GetDataValue(6, 0));
                            KSP ksp = new()
                            {
                                Code = index.ToString(),
                                Time = ComHelpers.AsLong(cls.GetDataValue(1, 0)),
                                Price = close,
                                Change = close - past,
                                Seq = DateTime.Now.Ticks,
                            };
                            OnReceive!(WorkType.Ksp, ksp);//.InsertKsp(ksp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _vm.AddLog($"업종 지수를 받는 중 에러 : {ex.Message}");
            }
        }
        private async void GetDataMst(string code)
        {
            try
            {
                FutureMst? fMst = new();
                fMst.SetInputValue(0, code);
                fMst.BlockRequest();

                await _connection!.WaitTime(nameof(GetDataMst));

                Master master = new()
                {
                    Code = ComHelpers.AsString(fMst.GetHeaderValue(0)),
                    Close = ComHelpers.AsDecimal(fMst.GetHeaderValue(71)),
                    Change = ComHelpers.AsDecimal(fMst.GetHeaderValue(77)),
                    Open = ComHelpers.AsDecimal(fMst.GetHeaderValue(72)),
                    High = ComHelpers.AsDecimal(fMst.GetHeaderValue(73)),
                    Low = ComHelpers.AsDecimal(fMst.GetHeaderValue(74)),
                    Volume = ComHelpers.AsLong(fMst.GetHeaderValue(75)),
                    OpenInterest = ComHelpers.AsLong(fMst.GetHeaderValue(80)),
                    PrevOpenInterest = ComHelpers.AsLong(fMst.GetHeaderValue(25)),
                    Time = ComHelpers.AsLong(fMst.GetHeaderValue(82)),
                    Turnover = ComHelpers.AsLong(fMst.GetHeaderValue(76)),
                    SellQuoteCount = ComHelpers.AsLong(fMst.GetHeaderValue(53)),
                    BuyQuoteCount = ComHelpers.AsLong(fMst.GetHeaderValue(70)),
                    Seq = DateTime.Now.Ticks,
                };
                OnReceive!(WorkType.Master, master);//.InsertMst(master);
            }
            catch (Exception ex)
            {
                _vm.AddLog($"선물 현재가 받는 중 에러 : {ex.Message}");
            }
        }
        private void FBid_Received()
        {
            try
            {
                BidsAsks ba = new()
                {
                    Code = ComHelpers.AsString(fBid!.GetHeaderValue(0)),
                    Time = ComHelpers.AsLong(fBid!.GetHeaderValue(1)),
                    AskPrice = ComHelpers.AsDecimal(fBid!.GetHeaderValue(2)),
                    BidPrice = ComHelpers.AsDecimal(fBid!.GetHeaderValue(19)),
                    AskTotalQuantity = ComHelpers.AsLong(fBid!.GetHeaderValue(12)),
                    BidTotalQuantity = ComHelpers.AsLong(fBid!.GetHeaderValue(29)),
                    MarketState = (byte)ComHelpers.AsLong(fBid!.GetHeaderValue(36)),
                    Ask1Quantity = ComHelpers.AsLong(fBid!.GetHeaderValue(7)),
                    Bid1Quantity = ComHelpers.AsLong(fBid!.GetHeaderValue(24)),
                    Seq = DateTime.Now.Ticks,
                };
                OnReceive!(WorkType.BidsAsks, ba);//.InsertBid(ba);
            }
            catch (Exception ex)
            {
                _vm.AddLog($"선물 호가 받는 중 에러 : {ex.Message}");
            }
        }
        private void FCur_Received()
        {
            try
            {
                FutureCur cur = new()
                {
                    Code = ComHelpers.AsString(fCur!.GetHeaderValue(0)),
                    Close = ComHelpers.AsDecimal(fCur!.GetHeaderValue(1)),
                    Change = ComHelpers.AsDecimal(fCur!.GetHeaderValue(2)),
                    Open = ComHelpers.AsDecimal(fCur!.GetHeaderValue(7)),
                    High = ComHelpers.AsDecimal(fCur!.GetHeaderValue(8)),
                    Low = ComHelpers.AsDecimal(fCur!.GetHeaderValue(9)),
                    Volume = ComHelpers.AsLong(fCur!.GetHeaderValue(13)),
                    OpenInterest = ComHelpers.AsLong(fCur!.GetHeaderValue(14)),
                    Time = ComHelpers.AsLong(fCur!.GetHeaderValue(15)),
                    SellVolume = ComHelpers.AsLong(fCur!.GetHeaderValue(22)),
                    BuyVolume = ComHelpers.AsLong(fCur!.GetHeaderValue(23)),
                    Turnover = ComHelpers.AsLong(fCur!.GetHeaderValue(26)),
                    TradeType = ComHelpers.AsLong(fCur!.GetHeaderValue(28)),
                    Basis = ComHelpers.AsDecimal(fCur!.GetHeaderValue(5)),
                    K200 = ComHelpers.AsDecimal(fCur!.GetHeaderValue(4)),
                    Seq = DateTime.Now.Ticks,
                };
                OnReceive!(WorkType.FutureCur, cur);//.InsertCur(cur);
            }
            catch (Exception ex)
            {
                _vm.AddLog($"선물 현재가 받는 중 에러 : {ex.Message}");
            }
        }
        private void FExc_Received()
        {
            try
            {
                string index = "4";
                KSP ksp = new()
                {
                    Code = index,
                    Time = ComHelpers.AsLong(fExc!.GetHeaderValue(1)),
                    Price = ComHelpers.AsDecimal(fExc!.GetHeaderValue(2)),
                    Change = ComHelpers.AsDecimal(fExc!.GetHeaderValue(3)),
                    Seq = DateTime.Now.Ticks,
                };
                OnReceive!(WorkType.Ksp, ksp);//.InsertKsp(ksp);
                Task.Delay(1);
            }
            catch (Exception ex)
            {
                _vm.AddLog($"선물 예상가 받는 중 에러 : {ex.Message}");
            }
        }

        private CpSvrNew7221S? _c7721s;
        private void StartTrade()
        {
            _vm.AddLog("투자자별 매매종합 작업 시작.");
            try
            {
                string[] v = ["U001", "F999", "0999", "0998"];
                _c7721s ??= new();
                _c7721s.Received += C7721s_Received;

                for (int i = 0; i < v.Length; i++)
                {
                    _c7721s.SetInputValue(0, v[i]);
                    _c7721s.SubscribeLatest();
                }
            }
            catch (Exception ex)
            {
                _vm.AddLog($"투자자별 매매 종합 작업 중 에러 : {ex.Message}");
            }
        }
        private void C7721s_Received()
        {
            try
            {
                // 0: 개인, 1: 외국인, 2: 기관
                for (int i = 0; i < 3; i++)
                {
                    Trading trade = new()
                    {
                        Code = ComHelpers.AsString(_c7721s!.GetHeaderValue(0)),
                        Time = ComHelpers.AsLong(_c7721s!.GetHeaderValue(1)),
                        Participant = i,
                        SellVolume = ComHelpers.AsLong(_c7721s!.GetDataValue(0, i)),
                        SellValue = ComHelpers.AsLong(_c7721s!.GetDataValue(1, i)),
                        BuyVolume = ComHelpers.AsLong(_c7721s!.GetDataValue(2, i)),
                        BuyValue = ComHelpers.AsLong(_c7721s!.GetDataValue(3, i)),
                        Seq = DateTime.Now.Ticks,
                    };
                    OnReceive!(WorkType.Trade, trade);
                    Task.Delay(1).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _vm.AddLog($"투자자별 매매 현황 접수 중 에러 : {ex.Message}");
            }
        }
        private async void FutureHighLow(string code)
        {
            try
            {
                // 0: 날짜, 2: 시가, 3:고가, 4: 저가, 5: 종가, 8: 거래량, 27: 미결제
                int[] ip = [0, 2, 3, 4, 5, 8, 27];
                FutOptChart cls = new();
                cls.SetInputValue(0, code);
                cls.SetInputValue(1, char.Parse("1"));

                if (_daishin!._currLimit >= DateTime.Today)
                    cls.SetInputValue(3, _daishin._postLimit);
                else
                    cls.SetInputValue(3, _daishin._currLimit);
                cls.SetInputValue(5, ip);
                cls.SetInputValue(6, char.Parse("D"));

                cls.BlockRequest();

                int length = (int)ComHelpers.AsLong(cls.GetHeaderValue(3));
                int today = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
                int past = int.Parse(_daishin._postLimit.ToString("yyyyMMdd"));
                for (int i = 0; i < length; i++)
                {
                    await _connection!.WaitTime(nameof(FutureHighLow));
                    int day = (int)ComHelpers.AsLong(cls.GetDataValue(0, i));
                    if (day < past) break;
                    if (day < today)
                    {
                        HighLow hl = new()
                        {
                            Code = ComHelpers.AsString(cls.GetHeaderValue(0)),
                            Date = ComHelpers.AsLong(cls.GetDataValue(0, i)),
                            Open = ComHelpers.AsDecimal(cls.GetDataValue(1, i)),
                            High = ComHelpers.AsDecimal(cls.GetDataValue(2, i)),
                            Low = ComHelpers.AsDecimal(cls.GetDataValue(3, i)),
                            Close = ComHelpers.AsDecimal(cls.GetDataValue(4, i)),
                            Volume = ComHelpers.AsLong(cls.GetDataValue(5, i)),
                            OpenInterest = ComHelpers.AsLong(cls.GetDataValue(6, i)),
                            Seq = DateTime.Now.Ticks,
                        };
                        OnReceive!(WorkType.HighLow, hl);//.InsertHighLow(hl);
                    }
                }
            }
            catch (Exception ex)
            {
                _vm.AddLog($"선물 고가 저가 입력 중 에러 : {ex.Message}");
            }
        }
        private void KospiHighLow(string code = "U180")
        {
            _vm.AddLog("코스피 주간 가격 작업 시작.");
            try
            {
                // 0 : 날짜, 2: 시가, 3: 고가, 4: 저가, 5: 종가, 8: 거래량
                int[] ip = [0, 2, 3, 4, 5, 8];
                StockChart cls = new();
                cls.SetInputValue(0, code);
                cls.SetInputValue(1, char.Parse("1"));

                if (_daishin?._currLimit >= DateTime.Today)
                    cls.SetInputValue(3, _daishin._postLimit);
                else
                    cls.SetInputValue(3, _daishin?._currLimit);
                cls.SetInputValue(5, ip);
                cls.SetInputValue(6, char.Parse("D"));

                cls.BlockRequest();

                int length = (int)ComHelpers.AsLong(cls.GetHeaderValue(3));
                for (int i = 0; i < length; i++)
                {
                    string date = ComHelpers.AsString(cls.GetDataValue(0, i));
                    if (int.Parse(date) < int.Parse(_daishin!._postLimit.ToString("yyyyMMdd")))
                        break;
                    if (int.Parse(date) < int.Parse(DateTime.Now.ToString("yyyyMMdd")))
                    {
                        HighLow hl = new()
                        {
                            Code = ComHelpers.AsString(cls.GetHeaderValue(0)),
                            Date = ComHelpers.AsLong(cls.GetDataValue(0, i)), // 날찌
                            Open = ComHelpers.AsDecimal(cls.GetDataValue(1, i)),
                            High = ComHelpers.AsDecimal(cls.GetDataValue(2, i)),
                            Low = ComHelpers.AsDecimal(cls.GetDataValue(3, i)),
                            Close = ComHelpers.AsDecimal(cls.GetDataValue(4, i)),
                            Volume = ComHelpers.AsLong(cls.GetDataValue(5, i)),
                            OpenInterest = 0,
                            Seq = DateTime.Now.Ticks,
                        };
                        OnReceive!(WorkType.HighLow, hl);
                    }
                }
            }
            catch (Exception ex)
            {
                _vm.AddLog($"KospiHighLow 작업 중 에러 발생 : {ex.Message}");
            }
        }
        private void GetCodes()
        {
            // ToDo
            // 당일 작업이 진행 되었다면, 진행 된 코드를 읽어오고,
            // 처음이라면 코드 작업부터 시작한다.
            GetFutureCode();
            GetOptionCode();
        }
        private void GetOptionCode()
        {
            try
            {
                CpOptionCode codes = new();
                OpCodes?.Clear();
                OpCodes = null;
                OpCodes = [];

                for (short i = 0; i < codes.GetCount(); i++)
                {
                    OptionCode code = new()
                    {
                        Code = codes.GetData(0, i).ToString(),
                        Name = codes.GetData(1, i).ToString(),
                        Classify = codes.GetData(2, i).ToString(),
                        Month = codes.GetData(3, i).ToString(),
                        DoPrice = ComHelpers.AsDecimal(codes.GetData(4, i).ToString()),
                        Seq = DateTime.Now.Ticks,
                    };
                    OpCodes?.Add(code);
                    OnReceive!(WorkType.OptionCode, code);
                }
            }
            catch (Exception ex)
            {
                _vm.AddLog($"GetOptionCode Error : {ex.Message}");
            }
        }
        private void GetFutureCode()
        {
            CpFutureCode codes = new();
            FuCodes?.Clear();
            FuCodes = null;
            FuCodes = [];

            try
            {
                for (short i = 0; i < codes.GetCount(); i++)
                {
                    FutureCode code = new()
                    {
                        Code = ComHelpers.AsString(codes.GetData(0, i)),
                        Name = ComHelpers.AsString(codes.GetData(1, i)),
                        Seq = DateTime.Now.Ticks,
                    };
                    FuCodes?.Add(code);
                    OnReceive!(WorkType.FutureCode, code);
                }
            }
            catch (Exception ex)
            {
                _vm.AddLog($"GetFutureCode Error : {ex.Message}");
            }
        }
        public void Dispose()
        {
            FuCodes?.Clear();
            FuCodes = null;
            OpCodes?.Clear();
            OpCodes = null;

            if (oCur != null)
            {
                oCur.Received -= OCur_Received;
                oCur = null;
            }
            if (fCur != null)
            {
                fCur.Received -= FCur_Received;
                fCur = null;
            }
            if (fBid != null)
            {
                fBid.Received -= FBid_Received;
                fBid = null;
            }
            if (sCur != null)
            {
                sCur = null;
            }
            if (kCur != null)
            {
                kCur = null;
            }
            if (fExc != null)
            {
                fExc.Received -= FExc_Received;
                fExc = null;
            }

            if (_c7721s != null)
            {
                _c7721s.Received -= C7721s_Received;
                _c7721s = null;
            }
        }
    }
}
