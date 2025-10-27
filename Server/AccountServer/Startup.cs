using AccountServer.DB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedDB;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using static StackExchange.Redis.Role;

namespace AccountServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public class PublicUrlOptions
        {
            public string BaseUrl { get; set; } = "";
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var cfg = Configuration;

            var acc = Environment.GetEnvironmentVariable("ConnectionStrings__AccountConnection")
                      ?? cfg.GetConnectionString("AccountConnection")
                      ?? Environment.GetEnvironmentVariable("AccountConnection"); // 과거 호환

            var shared = Environment.GetEnvironmentVariable("ConnectionStrings__SharedConnection")
                         ?? cfg.GetConnectionString("SharedConnection")
                         ?? Environment.GetEnvironmentVariable("SharedConnection"); // 과거 호환

            if (string.IsNullOrWhiteSpace(acc))
                throw new InvalidOperationException("AccountConnection이 설정되지 않았습니다.");
            if (string.IsNullOrWhiteSpace(shared))
                throw new InvalidOperationException("SharedConnection이 설정되지 않았습니다.");

            services.AddDbContext<AppDbContext>(o => o.UseSqlServer(acc, sql =>
            {
                sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            }));
            services.AddDbContext<SharedDbContext>(o => o.UseSqlServer(shared, sql =>
            {
                sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            }));

            var publicBaseUrl = Environment.GetEnvironmentVariable("ACCOUNTSERVER_PUBLIC_BASEURL")
                                ?? cfg["PublicBaseUrl"]   // appsettings.json 에서도 받을 수 있게
                                ?? "http://api.colorpickerstudio.com:51000/api/"; // 마지막 기본값(필요 시 교체)

            if (!publicBaseUrl.EndsWith("/")) publicBaseUrl += "/";
            services.AddSingleton(new PublicUrlOptions { BaseUrl = publicBaseUrl });

            var redisEndpoint = System.Environment.GetEnvironmentVariable("REDIS_ENDPOINT");   // host:port
            var redisPassword = System.Environment.GetEnvironmentVariable("REDIS_PASSWORD");   // AUTH token
            var useTls = (Environment.GetEnvironmentVariable("REDIS_USE_TLS") ?? "true")
                .Equals("true", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(redisEndpoint))
                throw new InvalidOperationException("REDIS_ENDPOINT가 비어 있습니다. EC2 환경변수 또는 appsettings에 설정하세요.");

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var options = ConfigurationOptions.Parse(redisEndpoint);
                if (!string.IsNullOrWhiteSpace(redisPassword)) options.Password = redisPassword;
                options.AbortOnConnectFail = false;
                options.ConnectRetry = 5;
                options.ConnectTimeout = 5000;
                options.KeepAlive = 30;
                if (useTls)
                {
                    options.Ssl = true;
                    options.SslProtocols = SslProtocols.Tls12;
                }
                return ConnectionMultiplexer.Connect(options);
            });

            services.AddCors(opt =>
            {
                opt.AddDefaultPolicy(p => p
                    .WithOrigins(
                        "http://api.colorpickerstudio.com", "http://api.colorpickerstudio.com:51000"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("{\"error\":\"An error occurred.\"}");
                    });
                });
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/health", async ctx => await ctx.Response.WriteAsync("OK"));
            });
        }
    }
}
