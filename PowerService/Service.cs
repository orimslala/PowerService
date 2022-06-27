using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Serilog;
using Serilog.Events;
using System.Configuration;
using System.IO;
using Services;
using CsvHelper;
using System.Globalization;
namespace PowerServiceChallenge
{
    public partial class Service : ServiceBase
    {
        private static IPowerService _service = new PowerService();

        private static Timer _timer = new Timer();

        public static Dictionary<int, string> PeriodsAsTime = new Dictionary<int, string>
        {
            {1, "23:00"},
            {2, "00:00"},
            {3, "01:00"},
            {4, "02:00"},
            {5, "03:00"},
            {6, "04:00"},
            {7, "05:00"},
            {8, "06:00"},
            {9, "07:00"},
            {10, "08:00"},
            {11, "09:00"},
            {12, "10:00"},
            {13, "11:00"},
            {14, "12:00"},
            {15, "13:00"},
            {16, "14:00"},
            {17, "15:00"},
            {18, "16:00"},
            {19, "17:00"},
            {20, "18:00"},
            {21, "19:00"},
            {22, "20:00"},
            {23 ,"21:00"},
            {24, "22:00"}
        };
        public Service()
        {
            InitializeComponent();
            if( double.TryParse(ConfigurationSettings.AppSettings["interval"], out double interval) )
            {
                _timer.Interval = interval * 60000;
                _timer.Elapsed += OnElapsedTime;
                _timer.AutoReset = true;
                _timer.Enabled = true;
            }
            else
            {
                Log.Error("Invalid scheduled interval value");
                throw new InvalidIntervalException("Invalid scheduled interval exception");
            }
            GetTrades();
        }


        private async Task  GetTrades()
        {
            try {

                var yesterday = DateTime.Now.AddDays(-1);
                yesterday = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 0, 0);
                Log.Information("Executing GetTrades for scheduled interval {A}", yesterday.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                IEnumerable<PowerTrade> periods = await _service.GetTradesAsync(yesterday).ConfigureAwait(false);
                var items = periods.ToList();
                var p = items.SelectMany(x => x.Periods)
                            .GroupBy(x => x.Period,
                                    x => x.Volume,
                                    (x, y) => new AggregatedPeriod { LocalTime = PeriodsAsTime[x], Volume = y.Sum() });
                var outputPath = ConfigurationSettings.AppSettings["outputdirectory"] + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

                Log.Information("Writing to output file {A}", outputPath);
                using (var writer = new StreamWriter(outputPath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<AggregatedPeriod>();
                    csv.NextRecord();
                    csv.WriteRecords<AggregatedPeriod>(p);
                }
                Log.Information("Finished executing GetTrades...");
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
        }

        private async void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            Log.Information("The Elapsed event was raised at {0:HH:mm:ss.fff}", e.SignalTime);

            await Task.Run(async () => await GetTrades()).ConfigureAwait(false);        
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}
