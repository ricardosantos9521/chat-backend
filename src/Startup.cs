using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using SignalRServer.Controllers;
using SignalRServer.SignalR;
using StackExchange.Redis;

namespace SignalRServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            string redis_config = Environment.GetEnvironmentVariable("REDIS_CONFIG");
            if (redis_config == null)
            {
                Console.WriteLine("Environment Variable 'REDIS_CONFIG' not present.");
            }

            Console.WriteLine($"\nREDIS_CONFIG: {redis_config}");

            services.AddMvc();

            services
                .AddSignalR(hubOptions =>
                {
                    hubOptions.EnableDetailedErrors = true;
                })
                // .AddMessagePackProtocol()
                .AddStackExchangeRedis(o =>
                {
                    o.ConnectionFactory = async writer =>
                    {
                        var connection = await ConnectionMultiplexer.ConnectAsync(redis_config, writer);

                        connection.ErrorMessage += (_, e) =>
                        {
                            Console.WriteLine("Error message from Redis: " + e);
                        };

                        connection.ConnectionFailed += (_, e) =>
                        {
                            Console.WriteLine("Connection to Redis failed: " + e);
                        };

                        if (!connection.IsConnected)
                        {
                            Console.WriteLine("Did not connect to Redis.");
                        }
                        else
                        {
                            Console.WriteLine("Connected to Redis.");
                        }
                        Console.WriteLine("");

                        return connection;
                    };
                });

            var redis = ConnectionMultiplexer.Connect(redis_config);
            var subscriber = redis.GetSubscriber();

            services.AddSingleton<ISubscriber>(subscriber);
            services.AddSingleton<SignalRServerComunication>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder =>
            {
                builder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed(isOriginAllowed: _ => true)
                        .AllowCredentials();
            });

            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/chat");
            });

            app.UseMvc();

            //var helper = app.ApplicationServices.GetService<SignalRServerComunication>();
            var subscriber = app.ApplicationServices.GetService<ISubscriber>();

            subscriber.SubscribeAsync("SendCount", async (channel, m) =>
            {
                Program.countGlobalUsers = 0;
                Console.WriteLine(string.Format("Restart users count - {0} usuários online local - {0} total usuários online.", ChatHub.countUsers, Program.countGlobalUsers));
                await subscriber.PublishAsync("CountUsers", ChatHub.countUsers);
            });

            subscriber.SubscribeAsync("CountUsers", (channel, m) =>
            {
                Int64 number = Int64.Parse(m.ToString());
                Program.countGlobalUsers += number;
                Console.WriteLine(string.Format("Add {0} users - {0} usuários online local - {0} total usuários online.", number, ChatHub.countUsers, Program.countGlobalUsers));
            });

            subscriber.PublishAsync("SendCount", "").Wait();

        }
    }
}
