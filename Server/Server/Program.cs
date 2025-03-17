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
        static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>();

        static void TickRoom(GameRoom room, int tick = 100)
        {
            var timer = new System.Timers.Timer();
            timer.Interval = tick;
            timer.Elapsed += ((s, e) => { room.Update(); });
            timer.AutoReset = true;
            timer.Enabled = true;

            _timers.Add(timer);
        }

        static void FlushRoom()
        {
            JobTimer.Instance.Push(FlushRoom, 250);
        }
        /*static void GameLogicTask()
		{
            while (true)
            {
                //GameLogic.Instance.Update();
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
		}*/

        static void Main(string[] args)
		{
            ConfigManager.LoadConfig();
            DataManager.LoadData();

            // TEST CODE
            /*using (AppDbContext db = new AppDbContext())
            {
                PlayerDb player = db.Players.FirstOrDefault();
                if (player != null)
                {
                    db.Achievements.Add(new AchievementDb()
                    {
                        TemplateId = 1,
                        IsActive = "true",
                        Slot = 0,
                        Owner = player
                    });

                    db.Achievements.Add(new AchievementDb()
                    {
                        TemplateId = 2,
                        IsActive = "false",
                        Slot = 1,
                        Owner = player
                    });

                    db.Achievements.Add(new AchievementDb()
                    {
                        TemplateId = 3,
                        IsActive = "false",
                        Slot = 2,
                        Owner = player
                    });

                    db.Achievements.Add(new AchievementDb()
                    {
                        TemplateId = 4,
                        IsActive = "true",
                        Slot = 5,
                        Owner = player
                    });

                    db.Achievements.Add(new AchievementDb()
                    {
                        TemplateId = 5,
                        IsActive = "false",
                        Slot = 6,
                        Owner = player
                    });

                    db.SaveChangesEx();
                }
            }*/

            GameRoom room = RoomManager.Instance.Add(1);
            TickRoom(room, 10);

            // DNS (Domain Name System)
            string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

            //FlushRoom();
            //JobTimer.Instance.Push(FlushRoom);

            while (true)
            {
                DbTransaction.Instance.Flush();
                //JobTimer.Instance.Flush();
                //Thread.Sleep(100);
            }
        }
	}
}
