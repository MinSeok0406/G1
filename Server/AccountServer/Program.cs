using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using AccountServer.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AccountServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // 최초 부팅 시 스키마 자동 생성/업데이트
            using (var scope = host.Services.CreateScope())
            {
                var sp = scope.ServiceProvider;

                void EnsureSchema<TContext>() where TContext : DbContext
                {
                    var ctx = sp.GetRequiredService<TContext>();
                    try { ctx.Database.Migrate(); }          // 마이그레이션이 있다면 적용
                    catch { ctx.Database.EnsureCreated(); }  // 없으면 최소 테이블 생성
                }

                // Account DB (dbo.Account)
                EnsureSchema<AppDbContext>();

                // Shared DB (dbo.Token, dbo.ServerInfo)
                EnsureSchema<SharedDB.SharedDbContext>();
            }

            host.Run();
        }

        // 환경설정 우선순위
        // 1) ASPNETCORE_URLS (예: http://0.0.0.0:51000;https://0.0.0.0:51001)
        // 2) PORT / LISTEN_HOST (환경변수 또는 구성값)
        // 3) 기본값: 0.0.0.0:51000
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    // appsettings.json / appsettings.{ENV}.json / 환경변수 / 커맨드라인 순차 포함
                    cfg.AddEnvironmentVariables();
                    if (args != null) cfg.AddCommandLine(args);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // 1) ASPNETCORE_URLS 최우선 (존재하면 Kestrel 수동 바인딩을 생략)
                    var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
                    if (!string.IsNullOrWhiteSpace(urls))
                    {
                        webBuilder.UseUrls(urls);
                    }
                    else
                    {
                        // 2) PORT / LISTEN_HOST 로 Kestrel 리슨 설정
                        var portStr = Environment.GetEnvironmentVariable("PORT")
                                      ?? webBuilder.GetSetting("PORT")
                                      ?? "51000";
                        if (!int.TryParse(portStr, out var port)) port = 51000;

                        var hostStr = Environment.GetEnvironmentVariable("LISTEN_HOST")
                                       ?? webBuilder.GetSetting("LISTEN_HOST")
                                       ?? "0.0.0.0";

                        if (!IPAddress.TryParse(hostStr, out var ip))
                            ip = IPAddress.Any; // 0.0.0.0

                        webBuilder.ConfigureKestrel(o =>
                        {
                            o.AddServerHeader = false; // Server 헤더 숨김
                            o.Listen(ip, port);
                            // 필요 시 HTTPS/추가 엔드포인트(o.Listen(IPAddress.Any, 51001, lo => lo.UseHttps());) 여기에 추가
                        });
                    }

                    webBuilder.UseStartup<Startup>();
                });
    }
}
