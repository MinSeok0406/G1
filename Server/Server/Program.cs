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
using Server.Data;
using Server.DB;
using Server.Game;
using ServerCore;
using SharedDB;

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
                DbTransaction.Instance.Flush();
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

        static void StartServerInfoTask()
        {
            var t = new System.Timers.Timer();
            t.AutoReset = true;
            t.Elapsed += new System.Timers.ElapsedEventHandler((s, e) =>
            {
                using (SharedDbContext shared = new SharedDbContext())
                {
                    ServerDb serverDb = shared.Servers.Where(s => s.Name == Name).FirstOrDefault();
                    if (serverDb != null)
                    {
                        serverDb.IpAddress = IpAddress;
                        serverDb.Port = Port;
                        serverDb.BusyScore = SessionManager.Instance.GetBusyScore();
                        shared.SaveChangesEx();
                    }
                    else
                    {
                        serverDb = new ServerDb()
                        {
                            Name = Program.Name,
                            Port = Program.Port,
                            IpAddress = Program.IpAddress,
                            BusyScore = SessionManager.Instance.GetBusyScore()
                        };

                        shared.Servers.Add(serverDb);
                        shared.SaveChangesEx();
                    }
                }
            });
            t.Interval = 10 * 1000;
            t.Start();
        }

        public static string Name { get; set; } = "빨강";
        public static int Port { get; set; } = 7777;
        public static string IpAddress { get; set; }

        static void Main(string[] args)
		{
            ConfigManager.LoadConfig();
            DataManager.LoadData();

            GameLogic.Instance.Push(() => { GameLogic.Instance.Add(1); });

            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, Port);

            IpAddress = ipAddr.ToString();

            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

            StartServerInfoTask();

            //FlushRoom();
            //JobTimer.Instance.Push(FlushRoom);

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
