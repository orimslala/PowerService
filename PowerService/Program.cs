
using System.ServiceProcess;
using CsvHelper.Configuration;
using Serilog;
using System.Configuration;
using System.IO;
using System;

namespace PowerServiceChallenge
{
    public class AggregatedPeriod
    {
        public string LocalTime { get; set; }
        public double Volume { get; set; }
    };


    public class AggregatedPeriodMap : ClassMap<AggregatedPeriod>
    {
        public AggregatedPeriodMap()
        {
            Map(m => m.LocalTime).Index(0).Name("localtime");
            Map(m => m.Volume).Index(1).Name("volume");
        }
    }
    static class Program
    {
            /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {

            string logFile = ConfigurationSettings.AppSettings["logfilelocation"];

            if (string.IsNullOrEmpty(logFile))
            {
                Log.Error("Appsettings parameter logfilelocation not found");
                throw new MissingLogFileConfigurationException("Appsettings parameter logfilelocation not found");
            }

            int pos = Path.GetFullPath(logFile).LastIndexOf("\\");
            if (!Directory.Exists(Path.GetFullPath(logFile).Substring(0, pos)))
            {
                Log.Error("File path is invalid");
                throw new InvalidFilePathException("File path is invalid");
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(Path.GetFullPath(logFile).Substring(0, pos) + "\\" + $"log_" +  DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt")
                .CreateLogger();
            ServiceBase[] ServicesToRun = new ServiceBase[]
            {
                new Service()
            };
            ServiceBase.Run(ServicesToRun);
            System.Console.ReadLine();
            Log.Information("application ending...");
           
        }
    }
}
