using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoLib;

namespace QueryWorker
{
    public static class QueryWorkerApp
    {
        public static void Main()
        {
            Console.WriteLine("QueryWorkerApp starting...");

            CloudFoundryMongoBinder binder = new CloudFoundryMongoBinder();

            MongoQueue<string> collectorQueue = new MongoQueue<string>(binder.Url, binder.DatabaseName, "collector_queue", 32000);
            MongoQueue<string> workerQueue = new MongoQueue<string>(binder.Url, binder.DatabaseName, "worker_queue", 32000);

            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting...");

                    string result = workerQueue.Receive();

                    Console.WriteLine("Received " + result);

                    collectorQueue.Send(binder.AppPort + ":" + result.ToUpper());

                    Console.WriteLine("Back to top of loop");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Query worker exiting after exception: " + e);
            }
        }
    }
}
