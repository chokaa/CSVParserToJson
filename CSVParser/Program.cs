using System;
using LumenWorks.Framework.IO.Csv;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Timers;

namespace CSVParser
{


    //The idea of storing data is to be stored in JSON file and to read/update it every 24 hours, in this example i did it every 60 seconds so i could test it
    //and because i didn't had service to call for data i tested it with files i included inside project ( test.csv and result is in output.json ). I stored data
    //inside json object because MongoDB is working with json-s ( bson-s thats like binary json ) so size is more/less size of Mongo collection which can store data 
    //for daily reports in next 5 years minimum. 

    //JsonFileReport is class that contains all blog_id -s and for every blog_id there is number of views/clicks in total and number of views/clicks on that day ( DailyClicks and DailyViews )
    //So when u start application it takes test.csv file, extract data from it and generate json with data formated like array of objects which contain blog_id and number of Clicks/DailyClicks and Views/DailyViews and date
    //so its easy to represent it or parse it further



    public class IdentificationReport
    {

        public int blogID { get; set; }

        public List<DailyReport> IdentificationData { get; set; }

        internal static void AddDailyReport(IdentificationReport id, DailyReport a)
        {
            id.IdentificationData.Add(a);
        }
        public IdentificationReport()
        {
            IdentificationData = new List<DailyReport>();
        }
    }

    public class DailyReport
    {

        public int blogID { get; set; }
        public string Date { get; set; }
        public int Clicks { get; set; }
        public int Views { get; set; }
        public int DailyClicks { get; set; }
        public int DailyViews { get; set; }

        
    }



    public class JsonFileReport {
        public List<IdentificationReport> Data { get; set; }

        public JsonFileReport()
        {
            Data = new List<IdentificationReport>();
        }
        internal static void AddIdentificationReport(JsonFileReport id, IdentificationReport a)
        {
            id.Data.Add(a);
        }


        public static void ParseCSVLumen(string fileLocation)
        {
            JsonFileReport JsonFile = new JsonFileReport();
            IdentificationReport IDReport = new IdentificationReport();
            DateTime today = DateTime.Today;
            using (CsvReader csv = new CsvReader(new System.IO.StreamReader(fileLocation), true))
            {

                csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;
                DailyReport currentDailyReport = new DailyReport();
                int fieldCount = csv.FieldCount;
                string[] headers = csv.GetFieldHeaders();

                while (csv.ReadNextRecord())
                {
                    for (int i = 0; i < fieldCount; i++)
                    {

                        if (headers[i] == "blogId")
                            currentDailyReport.blogID = Int32.Parse(csv[headers[i]]);
                        else if (headers[i] == "clicks")
                            currentDailyReport.Clicks = Int32.Parse(csv[headers[i]]);
                        else if (headers[i] == "views")
                            currentDailyReport.Views = Int32.Parse(csv[headers[i]]);
                        currentDailyReport.Date = today.ToString("yyyy-MM-dd");
                    }
                    

                    using (StreamReader readFile = File.OpenText(@"F:..\..\output.json"))
                    {
                        string json = readFile.ReadToEnd();
                        if (json == "")
                            JsonFile = new JsonFileReport();
                        else
                            JsonFile = JsonConvert.DeserializeObject<JsonFileReport>(json);
                        readFile.Close();
                    }


                    var FindOldIdentificationReport = new IdentificationReport();
                    if (JsonFile.Data.Count > 0)
                    {

                        if ((FindOldIdentificationReport = JsonFile.Data.Find(x => x.blogID == currentDailyReport.blogID)) != null)
                        {
                            currentDailyReport.DailyViews = currentDailyReport.Views - FindOldIdentificationReport.IdentificationData[FindOldIdentificationReport.IdentificationData.Count-1].Views;
                            currentDailyReport.DailyClicks = currentDailyReport.Clicks - FindOldIdentificationReport.IdentificationData[FindOldIdentificationReport.IdentificationData.Count-1].Clicks;

                            IdentificationReport.AddDailyReport(FindOldIdentificationReport, currentDailyReport);
                        }
                        else
                        {
                            currentDailyReport.DailyViews = currentDailyReport.Views;
                            currentDailyReport.DailyClicks = currentDailyReport.Clicks;

                            IdentificationReport NewIDReport = new IdentificationReport();
                            NewIDReport.blogID = currentDailyReport.blogID;
                            IdentificationReport.AddDailyReport(NewIDReport, currentDailyReport);
                            AddIdentificationReport(JsonFile, NewIDReport);
                        }
                    }
                    else
                    {
                        currentDailyReport.DailyViews = currentDailyReport.Views;
                        currentDailyReport.DailyClicks = currentDailyReport.Clicks;
                        IDReport.blogID = currentDailyReport.blogID;
                        IdentificationReport.AddDailyReport(IDReport, currentDailyReport);
                        AddIdentificationReport(JsonFile, IDReport);
                    }



                    StreamWriter file = File.CreateText(@"..\..\output.json");
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, JsonFile);
                    file.Close();

                    Console.Write(JsonConvert.SerializeObject(currentDailyReport));

                }

            }
        }


    }

    class Program
    {



        private static System.Timers.Timer aTimer;
        

        static void Main(string[] args)
        {
            
            SetTimer();

            Console.WriteLine("\nPress the Enter key to exit the application...\n");
            Console.WriteLine("The application started at {0:HH:mm:ss.fff}", DateTime.Now);
            Console.ReadLine();
            aTimer.Stop();
            aTimer.Dispose();

            Console.WriteLine("Terminating the application...");
            
        }

        private static void SetTimer()
        {
            //aTimer = new Timer( 24* 60*60* 1000);

            //Timer set to run extraction of data every minute so after running application wait for 60 seconds and it will extract data and store it inside output.json
            //I allready have done it 3 times and u can see how it works and how it stores data, u can run it several more times and change values from test.csv
            //So it will calculate number of dailyClicks/dailyViews from last time. When u want to stop application just hit enter key
            //Value should be changed to 24* 60*60* 1000 so it would run every day

            aTimer = new Timer( 60 * 1000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
                              e.SignalTime);
            JsonFileReport.ParseCSVLumen(@"..\..\test.csv");
        }
    }
}
