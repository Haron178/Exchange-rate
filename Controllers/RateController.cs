using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.Net.Http.Headers;
using System.Security.Cryptography.Xml;
using MessageBusFromKafka;
using Microsoft.AspNetCore.SignalR;
using System.Timers;
using System;
using System.Reflection.Metadata.Ecma335;

namespace My_First_Project.Controllers
{
    
    [ApiController]
    [Route("[controller]")]
    public class RateController : ControllerBase
    {
        //for kafka
        private static readonly string TopicRequest = "topicRequest";
        private static readonly string TopicResponse = "topicResponse";
        private enum Comands 
        {
            cadRub,
            audRub,
            gbpRub,
            usdRub,
            eurRub
        };
        public static string Message;
        public RateController() { }

        [HttpGet("get/{rate}")] ///{rate}
        public async Task<IActionResult> GetRate(string rate)
        {
            Message = null;
            using (var msgBus = new MessageBus())
            {
                if (!Enum.IsDefined(typeof(Comands), rate)) //rate
                    return BadRequest("command is not recognized");
                CancellationTokenSource source = new CancellationTokenSource();
                CancellationToken token = source.Token;
                Task.Run(() => msgBus.SubscribeOnTopic<string>(TopicResponse, msg => { GetMessageApi(msg); source.Cancel(); }, token));
                Thread.Sleep(50);
                msgBus.SendMessage(TopicRequest, rate);
                int timeIntervalCount = 0;
                while (Message == null)
                {
                    Thread.Sleep(10);
                    timeIntervalCount++;
                    if (timeIntervalCount >= 500) // waiting for a response by 5 second
                    {
                        source.Cancel();
                        return NoContent();
                    }
                }
            }
            return Ok(Message);
        }
        public static void GetMessageApi(string msg)
        {
            Message = msg;
        }
    }
}
