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

namespace Daenet.Common.SampleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
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
            })
                .Build();
    }
}
