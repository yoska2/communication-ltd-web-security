using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


//DB חיבור ל

namespace NEW_LTD.Data
{
    public static class DbConfig
    {
        public static readonly string ConnectionString =
    "Server=trolley.proxy.rlwy.net;" +
    "Port=53904;" +
    "Database=railway;" +
    "Uid=root;" +
    "Pwd=bFwDARhRWakPSVLarEyLrVCjwyORBSyQ;" + 
    "SslMode=Required;" +
    "Connection Timeout=60;";

    }

}
