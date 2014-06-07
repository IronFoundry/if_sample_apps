using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoLib;

namespace WebUI.Models
{
    public class Publisher
    {
        private MongoQueue<string> WorkerQueue { get; set; }

        public Publisher(MongoQueue<string> workerQueue)
        {
            WorkerQueue = workerQueue;
        }

        public void Publish(String userContent)
        {
            Console.WriteLine("writing " + userContent);
            WorkerQueue.Send(userContent);
            Console.WriteLine("Data written");
        }
    }
}