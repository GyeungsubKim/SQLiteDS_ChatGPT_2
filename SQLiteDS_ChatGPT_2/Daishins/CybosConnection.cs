using System.Windows;
using CPUTILLib;
using SQLiteDS_ChatGPT_2.Views;

namespace SQLiteDS_ChatGPT_2.Daishins
{
    internal class CybosConnection(ViewModel vm) : IDisposable
    {
        private readonly ViewModel _vm = vm;
        private CpCybos? _cybos;

        internal async Task WaitTime(string funName)
        {
            if (_cybos is null) return;
            if (_cybos.GetLimitRemainCount(LIMIT_TYPE.LT_NONTRADE_REQUEST) <= 0)
            {
                _vm.AddLog($"{funName} 서버대기 : " + DateTime.Now.ToString("HH:MM:ss") + ", " +
                              _cybos.LimitRequestRemainTime + ", " +
                              _cybos.GetLimitRemainCount(
                                 LIMIT_TYPE.LT_NONTRADE_REQUEST));
                do
                {
                    if (_cybos.LimitRequestRemainTime == 0) break;
                    string log = $"{funName} {_cybos!.LimitRequestRemainTime}";
                    _vm.AddLog(log);
                    await Task.Delay(15000);
                } while (true);

                string msg = DateTime.Now.ToString("HH:MM:ss") + ", " +
                             _cybos.LimitRequestRemainTime + ", " +
                             _cybos.GetLimitRemainCount(
                                LIMIT_TYPE.LT_NONTRADE_REQUEST);
                _vm.AddLog($"{funName} {msg}");
            }
        }
        public bool CybosConnected()
        {
            try
            {
                _cybos ??= new CpCybos();
                _cybos.OnDisconnect += Cybos_OnDisconnect;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cybos 연결이 안됩니다! {ex.Message}");
                return false;
            }
            if (_cybos != null && _cybos.IsConnect == 0)
                return false;
            return true;
        }
        public (DateTime, DateTime) GetTime()
        {
            _vm.AddLog($"{DateTime.Now:yyyy년 MM 월 dd일} 선물 옵션 데이터 작업 시작");
            DateTime open = DateTime.MinValue;
            DateTime close = DateTime.MinValue;

            try
            {
                CpCodeMgr mgr = new();
                string openTime = mgr.GetMarketStartTime().ToString();
                string closeTime = mgr.GetMarketEndTime().ToString();
                open = AddMinute(DateTime.Now, openTime, -15);
                close = AddMinute(DateTime.Now, closeTime, 15);
                _vm.AddLog($"시장 시작 시간 : {open:HHmm}, 마감 시간 : {close:HHmm}");
            }
            catch (Exception ex)
            {
                _vm.AddLog($"시장 시간 가져오기 Error : {ex.Message}");
            }
            return (open, close);
        }
        private static DateTime AddMinute(DateTime dt, string hm, int mm)
        {
            int h = int.Parse(hm[..2]);
            int m = int.Parse(hm[2..]);
            if (hm.Length < 4) h = int.Parse(hm[..1]);
            DateTime _dt = new(dt.Year, dt.Month, dt.Day, h, m, 0);
            _dt = _dt.AddMinutes(mm);
            return _dt;
        }
        internal void Cybos_OnDisconnect()
        {
            string msg =
                "CYBOS와 연결이 되어있지 않습니다!." +
                Environment.NewLine +
                "연결 후 이용 하시기 바랍니다..";
            MessageBox.Show(msg, "DisConnected", MessageBoxButton.OK);
        }
        /// <summary>
        /// 관리자 권한으로 실행 중인지 확인합니다.
        /// </summary>
        public bool IsUserAdmin()
        {
            // CYBOS Plus 통신을 위해서는 관리자 권한이 필수입니다.
            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        public void Dispose()
        {
            if (_cybos != null) _cybos = null;
        }
    }
}
