using Lamar;
using System;
using SpeedRunAppImport.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace SpeedRunAppImport
{
    public class Program
    {
        private static IConfiguration _config;

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
                .UseLamar(new ServiceAssemblyRegistry())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped<Processor>();
                })                
                .UseConsoleLifetime();
    }
}
