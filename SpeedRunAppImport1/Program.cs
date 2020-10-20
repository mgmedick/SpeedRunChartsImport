using Lamar;
using System;
using SpeedRunAppImport.Processor;
using SpeedRunAppImport.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Lamar.Microsoft.DependencyInjection;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace SpeedRunApp.Import
{
    public class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .UseServiceProviderFactory<ServiceRegistry>(new LamarServiceProviderFactory())
                .ConfigureContainer<Lamar.ServiceRegistry>((context, services) =>
                {
                    // Also exposes Lamar specific registrations
                    // and functionality
                    services.Scan(s =>
                    {
                        s.TheCallingAssembly();
                        s.Assembly("SpeedRunApp.Interfaces");
                        s.Assembly("SpeedRunApp.Service");
                        s.WithDefaultConventions();
                        s.SingleImplementationsOfInterface();
                    });
                });

        static void RunProcesses()
        {
            //Icontainer
        }

    }
}
