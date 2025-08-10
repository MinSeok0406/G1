using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AccountServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var portStr = Environment.GetEnvironmentVariable("PORT") ?? "51000"; // 원하는 포트
                    if (!int.TryParse(portStr, out var port)) port = 51000;

                    webBuilder.ConfigureKestrel(o =>
                    {
                        // ✅ 여기에만 바인딩(IPv4 Any). ASPNETCORE_URLS 설정이 있어도 이 설정이 우선됩니다.
                        o.Listen(IPAddress.Any, port);
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
