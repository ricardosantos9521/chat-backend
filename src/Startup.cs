using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace SignalRServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services
                .AddSignalR()
                .AddStackExchangeRedis(o =>
                {
                    o.ConnectionFactory = async writer =>
                    {
                        string redis_config = Environment.GetEnvironmentVariable("REDIS_CONFIG");
                        if (redis_config==null)
                        {
                            Console.WriteLine("Environment Variable 'REDIS_CONFIG' not present.");
                            return null;
                        }

                        Console.WriteLine($"\nREDIS_CONFIG: {redis_config}");

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

            // Configuro os hubs do SignalR
            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/chat");
            });

            app.UseMvc();

        }
    }
}
