using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Red_Alert_Engine
{
    internal class Program
    {
        public List<Alert> alert;
        public List<AlertHistory> json;
        private string location_ = "";
        private string date_ = "";
        private string title_ = "";
        private string website_ = "";
        private string alert_json = "";
        private string alert_json_ = "";
        private string desc_ = "";
        private string log_ = "";


        static async Task Main(string[] args)
        {
            Program program = new Program();
            program.Azaka().Start();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public struct Alert
        {
            public string desc;
            public string title;
            public List<string> data;

            public Alert(string desc, string title, List<string> data)
            {
                this.desc = desc;
                this.title = title;
                this.data = data;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public struct AlertHistory
        {
            public string alertDate;
            public string title;
            public string data;

            public AlertHistory(string alertDate, string title, string data)
            {
                this.alertDate = alertDate;
                this.title = title;
                this.data = data;
            }
        }

        public async Task RedAlert()
        {
            Console.WriteLine("In: " + location_);
            Console.WriteLine("descrption: " + desc_);
            Console.WriteLine("title" + title_);
            Console.WriteLine("date" + date_);
        }

        private static T GetNestedException<T>(Exception ex) where T : Exception
        {
            if (ex == null)
            {
                return null;
            }

            var tEx = ex as T;
            if (tEx != null)
            {
                return tEx;
            }

            return GetNestedException<T>(ex.InnerException);
        }

        private async Task Reconnect_ToServer()
        {
            while (true)
            {
                try
                {
                    Task.Delay(60000).Wait(60000);
                    using (WebClient clinet = new WebClient())
                    {
                        clinet.Headers.Add("user-agent",
                            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36");
                        clinet.Headers.Add("X-Requested-With", "XMLHttpRequest");
                        clinet.Headers.Add("Referer", "https://www.oref.org.il/11226-he/pakar.aspx");

                        alert_json =
                            clinet.DownloadString("https://www.oref.org.il/WarningMessages/alert/alerts.json");
                    }

                    // if to here it works means server responding agian, should restart the task.
                    Azaka().Dispose();
                    ///////////////////////////////////////////
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.ForegroundColor = ConsoleColor.White;
                    log_ = log_ + "\n" + ( "Red Alert"+
                        " Reconnected to main server! - restarting task.");
                    Console.WriteLine();
                    Console.WriteLine("Red Alert" +
                        " Reconnected to main server! - restarting task.");
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    ///////////////////////////////////////////
                    await Azaka();
                    break;
                }
                catch (Exception)
                {
                    //continue running until recoonect.
                }
            }
        }

        public async Task Azaka() //need to add delay
        {
            Alert alert = JsonConvert.DeserializeObject<Alert>("{}");
            var old_alert = alert;

            /////////////////////////////////////////// create object
            Task.Delay(1500).Wait(1500);
            ///////////////////////////////////////////
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            log_ = log_ + "\n" + ( "Red Alert" + " Started Looking For Red Alerts!");
            Console.WriteLine();
            Console.WriteLine("Red Alert" + " Started Looking For Red Alerts!");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            ///////////////////////////////////////////

            while (true)
            {
                try
                {
                    ////////////////////////////////////////////
                    //Console.WriteLine("Still Looking....");
                    ////////////////////////////////////////////
                    using (WebClient clinet = new WebClient())
                    {
                        clinet.Headers.Add("user-agent",
                            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36");
                        clinet.Headers.Add("X-Requested-With", "XMLHttpRequest");
                        clinet.Headers.Add("Referer", "https://www.oref.org.il/11226-he/pakar.aspx");

                        alert_json = clinet.DownloadString("https://www.oref.org.il/WarningMessages/alert/alerts.json");
                    }

                    ///////////////////////////////////////////
                    Task.Delay(2500).Wait(2500);
                    ///////////////////////////////////////////
                    using (WebClient clinet = new WebClient())
                    {
                        clinet.Headers.Add("user-agent",
                            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36");
                        clinet.Headers.Add("X-Requested-With", "XMLHttpRequest");
                        clinet.Headers.Add("Referer", "https://www.oref.org.il/11226-he/pakar.aspx");
                        alert_json_ = clinet.DownloadString("https://www.oref.org.il/WarningMessages/alert/alerts.json");
                    }
                }
                catch (Exception ex)
                {
                    log_ = log_ + (ex);
                    Console.WriteLine(ex);
                    var wex = GetNestedException<WebException>(ex);

                    // If there is no nested WebException, re-throw the exception.
                    //if (wex == null) { throw; }

                    // Get the response object.
                    var response = wex.Response as HttpWebResponse;

                    // If it's an HTTP response
                    if (response != null)
                    {
                        _ = Reconnect_ToServer();
                        break;
                    }

                    ///////////////////////////////////////////
                    Task.Delay(2500).Wait(2500);
                    ///////////////////////////////////////////
                }

                if (alert_json_ != alert_json && alert_json_ != null)
                {
                    location_ = "";
                    desc_ = "";
                    title_ = "";
                    website_ = "";

                    try
                    {
                        alert = JsonConvert.DeserializeObject<Alert>(alert_json_);
                        old_alert = JsonConvert.DeserializeObject<Alert>(alert_json);

                        alert.data = alert.data.Except(old_alert.data).ToList();
                    }
                    catch
                    {
                    }

                    try
                    {
                        foreach (string loc in alert.data)
                        {
                            location_ = location_ + " " + loc;
                        }

                        desc_ = ("הוראות: " + alert.desc);
                        title_ = ("אירוע: " + alert.title);
                        website_ = (@"https://www.google.com/maps/search/" + alert.data[0].Replace(" ", "_"));

                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;

                        log_ = log_ + "\n" + ("Red Alert" + " Red Alert Detected!");
                        Console.WriteLine("Red Alert" + " Red Alert Detected!");

                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;

                        await RedAlert();
                    }
                    catch
                    {
                    }
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// ------ OLD

            using (WebClient clinet = new WebClient())
            {
                clinet.Headers.Add("user-agent",
                    "Mozilla / 5.0(Windows NT 10.0; Win64; x64; rv: 99.0) Gecko / 20100101 Firefox / 99.0");
                //json = JsonConvert.DeserializeObject<List<Alert>>(clinet.DownloadString("https://www.oref.org.il/WarningMessages/History/AlertsHistory.json"));
                ///////////////////////////////////////////
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                log_ = log_ + "\n" + ("Red Alert" +
                    " Reverted to back-up systems! - Problem in server.");
                Console.WriteLine("Red Alert" +
                    " Reverted to back-up systems! - Problem in server.");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                ///////////////////////////////////////////
                while (true)
                {
                    try
                    {
                        ////////////////////////////////////////////
                        //Console.WriteLine("Still Looking....");
                        ////////////////////////////////////////////

                        json = JsonConvert.DeserializeObject<List<AlertHistory>>(
                            clinet.DownloadString(
                                "https://www.oref.org.il/WarningMessages/History/AlertsHistory.json"));
                        date_ = ("תאריך: " + json[0].alertDate);

                        ///////////////////////////////////////////
                        Task.Delay(5500).Wait(5500);
                        ///////////////////////////////////////////

                        json = JsonConvert.DeserializeObject<List<AlertHistory>>(
                            clinet.DownloadString(
                                "https://www.oref.org.il/WarningMessages/History/AlertsHistory.json"));
                        location_ = (json[0].data);
                        title_ = ("אירוע: " + json[0].title);

                        string new_date = ("תאריך: " + json[0].alertDate);
                        if (new_date != date_)
                        {
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.ForegroundColor = ConsoleColor.White;
                            log_ = log_ + "\n" + ("Red Alert" + " Red Alert Detected!");
                            Console.WriteLine("Red Alert" + " Red Alert Detected!");
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            website_ = (@"https://www.google.com/maps/search/" + json[0].data.Replace(" ", "_"));
                            //await RedAlert_Id();  --- ads quite a bit of delay should be better off. - should do on server leave
                            await RedAlert();
                            //Console.WriteLine(location + "\n" + date + "\n" + title);
                        }
                    }
                    catch (Exception e)
                    {
                        ///////////////////////////////////////////
                        Task.Delay(5500).Wait(5500);
                        ///////////////////////////////////////////

                        //Console.BackgroundColor = ConsoleColor.Red;
                        //Console.ForegroundColor = ConsoleColor.White

                        //Console.WriteLine("Error!\n" + e);
                        log_ = log_ + (e);
                    }
                }
            }
        }
    }
}
