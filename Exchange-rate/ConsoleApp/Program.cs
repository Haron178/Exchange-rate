// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApp;
using MessageBusLib;
using Microsoft.VisualBasic;
using StackExchange.Redis;

namespace ConsoleApp1
{
    class Program
    {
        //for kafka
        private static readonly string TopicRequest = "topicRequest";
        private static readonly string TopicResponse = "topicResponse";
        private static readonly Dictionary<string, int> RequestDictianary = new Dictionary<string, int> 
        { 
            {"test",0},
            {"cadRub", 0},
            {"audRub", 1},
            {"gbpRub", 2},
            {"usdRub", 3},
            {"eurRub", 4}
        };
        //for parsing
        private static List<string> URLList = new List<string>
        {
            "https://ru.investing.com/currencies/cad-rub",
            "https://ru.investing.com/currencies/aud-rub",
            "https://ru.investing.com/currencies/gbp-rub",
            "https://ru.investing.com/currencies/usd-rub",
            "https://ru.investing.com/currencies/eur-rub"
        };
        private static string urlParameters = "?api_key=123";
        //for Redis
        private static List<string>? ListTime;
        static void Main(string[] args)
        {
            ListTime = new List<string>();
            Task.Run(() =>
            {
                using (var redControl = new RedisController(ListTime))
                {
                    while (true)
                    {
                        var dataList = new List<string>();
                        foreach (var url in URLList)
                        {
                            var data = GetDataFromUrl(url);
                            dataList.Add(data);
                        }
                        ListTime.Add(DateTime.Now.ToString());
                        redControl.SetList(dataList);
                        WriteOnConsole(dataList);
                        Thread.Sleep(60000);
                    }
                }
            });
            using (var msgBus = new MessageBus())
            {
                msgBus.SubscribeOnTopic<string>(TopicRequest, msg => GetComandAndSendResponse(msg), CancellationToken.None);//заменить делегат
 
            }
        }

        public static void GetComandAndSendResponse(string msg)
        {
            if (RequestDictianary.ContainsKey(msg))
            {
                try 
                {
                    var comand = RequestDictianary[msg];
                    string message = "";
                    var listData = new List<string>();
                    using (var redControl = new RedisController(ListTime))
                    {
                        listData = redControl.GetListByKeys(Enumerable.Reverse(ListTime).Take(10))
                            .Select(redVal => redVal.ToString())
                            .ToList();
                    }
                    message = listData.Select(data => Convert.ToDecimal(data.Split('\n')[comand])).Average().ToString();
                    using (var msgBus = new MessageBus())
                    {
                        msgBus.SendMessage(TopicResponse, message);
                    }
                    Console.WriteLine($"Message: '{msg}' delivered");
                } 
                catch 
                {
                    Console.WriteLine("Message not delevired");
                }

            }
            else
            {
                Console.WriteLine("Comand not recognized");
            }
        }

        public static string GetDataFromUrl(string url)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
                var responseMessage = client.GetAsync(urlParameters).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
                if (responseMessage.IsSuccessStatusCode)
                {
                    
                    var html = responseMessage.Content.ReadAsStringAsync().Result;// Make sure to add a reference to System.Net.Http.Formatting.dll
                    return ParsingFromHtml(html);
                }
                else
                {
                    Console.WriteLine("{0} ({1})", (int)responseMessage.StatusCode, responseMessage.ReasonPhrase);
                }
                return null;
            }
        }
        public static string ParsingFromHtml(string html)
        {
            if (!string.IsNullOrEmpty(html)) 
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                var table = doc.DocumentNode.SelectSingleNode("/html/body/div/div[2]/div/div/div[2]/main/div/div[1]/div[2]/div[1]/span");
                if (table == null) 
                { 
                    throw new Exception("problems with parsing"); 
                }
                return table.InnerText.Trim();
            }
            throw new Exception("html is null");
        }

        public static void WriteOnConsole(List<string> list)
        {
            foreach (var item in list)
            {
                Console.Write(item);
                Console.Write("  ");
            }
            Console.WriteLine();
        }
    }
}
