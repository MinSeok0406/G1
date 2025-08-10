using AccountServer.DB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var cfg = Configuration;

            string acc = Environment.GetEnvironmentVariable("AccountConnection")
                         ?? cfg.GetConnectionString("AccountConnection");
            string shared = Environment.GetEnvironmentVariable("SharedConnection")
                            ?? cfg.GetConnectionString("SharedConnection");

            services.AddDbContext<AppDbContext>(o => o.UseSqlServer(acc));
            services.AddDbContext<SharedDbContext>(o => o.UseSqlServer(shared));

            var redisEndpoint = System.Environment.GetEnvironmentVariable("REDIS_ENDPOINT");   // host:port
            var redisPassword = System.Environment.GetEnvironmentVariable("REDIS_PASSWORD");   // AUTH token
            var useTls = (System.Environment.GetEnvironmentVariable("REDIS_USE_TLS") ?? "true").ToLower() == "true";

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
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
