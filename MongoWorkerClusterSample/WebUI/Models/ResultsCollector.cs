using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using MongoLib;

namespace WebUI.Models
{
    public class ResultsCollector : Hub
    {
        public void SendResults()
        {
            CloudFoundryMongoBinder binder = new CloudFoundryMongoBinder();
            MongoQueue<string> collectorQueue = new MongoQueue<string>(binder.Url, binder.DatabaseName, "collector_queue", 32000);

            while (true)
            {
                Console.WriteLine("Waiting for data...<");

                string userData = collectorQueue.Receive();

                Console.WriteLine(userData + "> writing to client...");

                Clients.All.addNewMessageToPage(userData);

                Console.WriteLine("data written");
            } 
        }
    }
}