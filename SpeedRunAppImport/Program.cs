using Lamar;
using Serilog;
using Serilog.Extensions.Hosting;
using Serilog.Sinks.Email;
using System;
using SpeedRunAppImport.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog.Settings.Configuration;

namespace SpeedRunAppImport
{
    public class Program
    {
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                var processor = services.GetRequiredService<Processor>();
                processor.RunProcesses();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseLamar()
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddCommandLine(args);
                })
                .ConfigureContainer<Lamar.ServiceRegistry>((context, services) =>
                {
                    services.AddScoped<Processor>();
                    services.Scan(s =>
                    {
                        s.TheCallingAssembly();
                        s.Assembly("SpeedRunAppImport.Repository");
                        s.Assembly("SpeedRunAppImport.Service");
                        s.Assembly("SpeedRunAppImport.Interfaces");
                        s.WithDefaultConventions();
                        s.SingleImplementationsOfInterface();
                    });
                })
                .UseConsoleLifetime();
    }
}
