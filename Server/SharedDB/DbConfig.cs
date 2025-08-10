using System;
using System.Collections.Generic;
using System.Text;

namespace SharedDB
{
    public static class DbConfig
    {
        public const string GameConnection =
            "Server=tcp:gameserverdb.cnksqwoqm9qg.ap-northeast-2.rds.amazonaws.com,1433;Database=G1GameDB;User ID=admin;Password=wltjd456!@#$;Encrypt=True;TrustServerCertificate=False;";

        public const string SharedConnection =
            "Server=tcp:gameserverdb.cnksqwoqm9qg.ap-northeast-2.rds.amazonaws.com,1433;Database=G1SharedDB;User ID=admin;Password=wltjd456!@#$;Encrypt=True;TrustServerCertificate=False;";
    }
}
