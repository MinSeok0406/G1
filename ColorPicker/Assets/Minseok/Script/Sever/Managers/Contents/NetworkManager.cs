using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Google.Protobuf;

namespace Minseok
{
    public class NetworkManager
    {
        public int AccountId { get; set; }
        public int Token { get; set; }
        public string GoogleID { get; set; }
        public string UserName { get; set; }

        ServerSession _session = new ServerSession();

        public void Send(IMessage packet)
        {
            _session.Send(packet);
        }

        public void ConnectToGame(ServerInfo info)
        {
            IPAddress ipAddr;
            if (!IPAddress.TryParse(info.IpAddress, out ipAddr))
            {
                var addrs = Dns.GetHostAddresses(string.IsNullOrWhiteSpace(info.IpAddress)
                    ? ServerConfig.GameHostFallback
                    : info.IpAddress);

                ipAddr = addrs.First(a => a.AddressFamily == AddressFamily.InterNetwork); // IPv4
            }

            int port = (info.Port > 0) ? info.Port : ServerConfig.GamePortFallback;
            IPEndPoint endPoint = new IPEndPoint(ipAddr, info.Port);

            Connector connector = new Connector();

            connector.Connect(endPoint,
                () => { return _session; },
                1);
        }

        public void Update()
        {
            List<PacketMessage> list = PacketQueue.Instance.PopAll();
            foreach (PacketMessage packet in list)
            {
                Action<PacketSession, IMessage> handler = PacketManager.Instance.GetPacketHandler(packet.Id);
                if (handler != null)
                    handler.Invoke(_session, packet.Message);
            }
        }

    }
}