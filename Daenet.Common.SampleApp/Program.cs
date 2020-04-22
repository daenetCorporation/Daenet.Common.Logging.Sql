using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Daenet.Common.Logging.Sql;
using Microsoft.Extensions.Hosting;

namespace Daenet.Common.SampleApp
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
                    webBuilder.UseStartup<Startup>()
        .ConfigureLogging((hostingContext, logging) =>
        {
            var loggerSection = hostingContext.Configuration.GetSection("Logging");
            logging.AddConfiguration(loggerSection);
            logging.AddConsole();
            logging.AddDebug();
            logging.AddSqlServerLogger((sett) =>
            {
                sett.SetSqlServerLoggerSettings(loggerSection);
            });
        });
                });
    }
}

