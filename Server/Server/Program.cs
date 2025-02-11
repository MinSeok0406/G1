using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server.Game;
using ServerCore;

namespace Server
{
	// 1. GameRoom 방식의 간단한 동기화
	// 2. 더 넓은 영역 관리
	// 3. 심리스 MMO

	// 1. Recv (멀티스레드 n개)
	// 2. GameLogic (단일스레드)
	// 3. Send (단일스레드)
	// 4. DB (단일스레드)

    class Program
	{
		static Listener _listener = new Listener();

		static void GameLogicTask()
		{
            while (true)
            {
                GameLogic.Instance.Update();
                Thread.Sleep(0);
            }
        }

		static void DbTask()
		{
            while (true)
            {
				Thread.Sleep(0);
            }
        }

		static void NetworkTask()
		{
			while (true)
			{
				List<ClientSession> sessions = SessionManager.Instance.GetSessions();
                foreach (ClientSession session in sessions)
                {
					session.FlushSend();
                }

                Thread.Sleep(0);
			}
		}

		static void Main(string[] args)
		{
            GameLogic.Instance.Push(() =>
            {
                GameLogic.Instance.Add(1);
            });

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

            // DbTask -> 직원 채용 후 일을 시킴
            {
                Thread t = new Thread(DbTask);
				t.Name = "DB";
				t.Start();
			}

            // NetworkTask -> 직원 채용 후 일을 시킴
            {
                Thread t = new Thread(NetworkTask);
                t.Name = "Network Send";
                t.Start();
            }

			// GameLogic -> 직원 채용 후 일을 시킴
			Thread.CurrentThread.Name = "GameLogic";
            GameLogicTask();
		}
	}
}
