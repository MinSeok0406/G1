using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minseok
{
    // 클라에서 서버로 보냄
    public class CreateAccountPacketReq
    {
        public string AccountName;
    }

    // 서버에서 클라로 보냄
    public class CreateAccountPacketRes
    {
        public bool CreateOk;
    }

    public class LoginAccountPacketReq
    {
        public string AccountName;
    }

    public class ServerInfo
    {
        public string Name;
        public string IpAddress;
        public int Port;
        public int BusyScore;
    }

    public class LoginAccountPacketRes
    {
        public bool LoginOk;
        public int AccountId;
        public int Token;
        public List<ServerInfo> ServerList = new List<ServerInfo>();
    }
}