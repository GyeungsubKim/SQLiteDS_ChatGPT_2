using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteDS_ChatGPT_2.Workers
{
    class DbRouterWorker
    {
        public Task RunAsync() => Task.CompletedTask;
        // 필요 시: WriteChannel 읽어서 SymbolId별 큐로 분배
    }
}
