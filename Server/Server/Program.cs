using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Server.DB;
using Server.Game;
using ServerCore;
using SharedDB;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();

        // 🔧 Public 주소/포트(클라에게 내려줄 값)
        public static string PublicHost { get; set; } = "game.colorpickerstudio.com"; // 예시
        public static int Port { get; set; } = 50000;

        // 🔧 Bind 주소(소켓 리슨용)
        public static IPAddress BindAddress { get; set; } = IPAddress.Any;

        public static string Name { get; set; } = "gameserver-1";

        static void EnsureGameSchemas()
        {
            // Game DB (dbo.Account, dbo.Player, dbo.Achievement 등)
            try
            {
                using (var game = new Server.DB.AppDbContext())
                {
                    try { game.Database.Migrate(); }
                    catch { game.Database.EnsureCreated(); }   // 여기에 Achievement도 같이 생성됨
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[SCHEMA] GameDb ensure failed: " + e.Message);
            }

            // Shared DB (토큰/서버리스트 – 콘솔에서도 쓰니까 함께 보장)
            try
            {
                using (var shared = new SharedDB.SharedDbContext())
                {
                    try { shared.Database.Migrate(); }
                    catch { shared.Database.EnsureCreated(); }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[SCHEMA] SharedDb ensure failed: " + e.Message);
            }
        }

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
                    session.FlushSend();

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
                    ServerDb serverDb = shared.Servers.Where(sv => sv.Name == Name).FirstOrDefault();
                    if (serverDb != null)
                    {
                        serverDb.IpAddress = PublicHost;             // 🔧 DB에는 PublicHost 저장
                        serverDb.Port = Port;
                        serverDb.BusyScore = SessionManager.Instance.GetBusyScore();
                        shared.SaveChangesEx();
                    }
                    else
                    {
                        serverDb = new ServerDb()
                        {
                            Name = Name,
                            IpAddress = PublicHost,                   // 🔧
                            Port = Port,
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

        static void ConfigureBackends()
        {
            // --- Redis ---
            var redisEndpoint = Environment.GetEnvironmentVariable("REDIS_ENDPOINT"); // "master....:6379"
            var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD");
            var useTls = (Environment.GetEnvironmentVariable("REDIS_USE_TLS") ?? "true")
                          .Equals("true", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(redisEndpoint))
                Console.WriteLine("[WARN] REDIS_ENDPOINT not set. Auth fast-path will be disabled.");

            try
            {
                if (!string.IsNullOrWhiteSpace(redisEndpoint))
                {
                    var opt = ConfigurationOptions.Parse(redisEndpoint);
                    if (!string.IsNullOrWhiteSpace(redisPassword)) opt.Password = redisPassword;
                    opt.AbortOnConnectFail = false;
                    opt.ConnectRetry = 5;
                    opt.ConnectTimeout = 5000;
                    opt.KeepAlive = 30;
                    if (useTls) opt.Ssl = true;

                    var mux = ConnectionMultiplexer.Connect(opt);
                    ClientSession.RedisDb = mux.GetDatabase();
                    Console.WriteLine("[OK] Redis connected");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERR] Redis connect failed: " + e.Message);
            }

            // --- SharedDB (EF Core) ---
            var sharedConn =
                Environment.GetEnvironmentVariable("ConnectionStrings__SharedConnection")
                ?? Environment.GetEnvironmentVariable("SharedConnection");
            if (string.IsNullOrWhiteSpace(sharedConn))
                Console.WriteLine("[WARN] SharedConnection not set. Token DB fallback will be disabled.");

            ClientSession.SharedDbFactory = () =>
            {
                var builder = new DbContextOptionsBuilder<SharedDbContext>();
                builder.UseSqlServer(sharedConn, sql =>
                {
                    sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                });
                return new SharedDbContext(builder.Options);
            };
        }

        static void LoadEnv()
        {
            // 작업 디렉터리 고정(서비스 실행 시 상대경로 이슈 방지)
            System.IO.Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            Name = Environment.GetEnvironmentVariable("GAMESERVER_NAME") ?? Name;

            var portStr = Environment.GetEnvironmentVariable("GAMESERVER_PORT")
                          ?? Environment.GetEnvironmentVariable("PORT");
            if (int.TryParse(portStr, out var p)) Port = p;

            // 🔧 PublicHost: 클라에 내려줄 주소(도메인/IP)
            PublicHost =
                Environment.GetEnvironmentVariable("GAMESERVER_PUBLIC_HOST")
                ?? Environment.GetEnvironmentVariable("PUBLIC_HOST")   // 기존 키도 허용
                ?? Environment.GetEnvironmentVariable("PUBLIC_IP")
                ?? PublicHost;

            // 🔧 BindAddress: 소켓 바인딩용 IP
            var bindHost = Environment.GetEnvironmentVariable("GAMESERVER_BIND_HOST");
            if (!string.IsNullOrWhiteSpace(bindHost) && IPAddress.TryParse(bindHost, out var parsed))
                BindAddress = parsed; // ex) 0.0.0.0, 127.0.0.1, 특정 NIC IP
            else
                BindAddress = IPAddress.Any;
        }

        static void Main(string[] args)
        {
            LoadEnv();
            EnsureGameSchemas();

            // 백엔드(REDIS / SharedDB)
            ConfigureBackends();

            GameLogic.Instance.Push(() => { GameLogic.Instance.Add(1); });

            // 🔧 바인딩은 BindAddress, 포트는 Port
            IPEndPoint endPoint = new IPEndPoint(BindAddress, Port);
            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });

            Console.WriteLine($"Listening on {BindAddress}:{Port} (public: {PublicHost}:{Port})"); // 🔧

            StartServerInfoTask();

            // DbTask
            {
                Thread t = new Thread(DbTask) { Name = "DB" };
                t.Start();
            }

            // NetworkTask
            {
                Thread t = new Thread(NetworkTask) { Name = "Network Send" };
                t.Start();
            }

            // GameLogic
            Thread.CurrentThread.Name = "GameLogic";
            GameLogicTask();
        }
    }
}
