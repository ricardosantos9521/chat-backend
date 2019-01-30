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
        public static int Rediness = 0;
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
                    hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
                    hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(30);
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
                            Rediness++;
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

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(5000);
                Console.WriteLine("\n");

                var subscriber = app.ApplicationServices.GetService<ISubscriber>();

                subscriber.Subscribe("SendCount", (channel, m) =>
                {
                    ChatHub.countGlobalUsers = 0;
                    Console.WriteLine("Restart users count - {0} local online - {0} total online", ChatHub.countUsers, ChatHub.countGlobalUsers);
                    subscriber.Publish("CountUsers", ChatHub.countUsers);
                });

                subscriber.Subscribe("CountUsers", (channel, m) =>
                {
                    Int64 number = Int64.Parse(m.ToString());
                    ChatHub.countGlobalUsers = ChatHub.countGlobalUsers + number;
                    Console.WriteLine(string.Format("Added {0} users - {0} local online - {0} total online", number, ChatHub.countUsers, ChatHub.countGlobalUsers));
                });

                //administrator controlles the SendCount requests
                var subscrivers = subscriber.Publish("administrator", String.Empty);
                if (subscrivers == 0)
                {
                    Console.WriteLine("I'm an administrator");
                    var helper = app.ApplicationServices.GetService<SignalRServerComunication>();
                    var semaphore = new Semaphore(0, 1);
                    semaphore.Release();
                    subscriber.Subscribe("administrator", async (channel, m) =>
                    {
                        semaphore.WaitOne();
                        subscriber.Publish("SendCount", String.Empty);
                        Thread.Sleep(5000);
                        semaphore.Release();
                        await helper.Send(ChatHub.countGlobalUsers);
                    });
                    subscriber.Publish("administrator", String.Empty);
                }
                Rediness++;
            });

        }
    }
}
