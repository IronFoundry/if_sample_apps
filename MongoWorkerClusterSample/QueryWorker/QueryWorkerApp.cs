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
            MongoQueue<string> workerQueue = new MongoQueue<string>("mongodb://127.0.0.1/queue_test", "queue_test", "worker_queue", 32000);
            MongoQueue<string> collectorQueue = new MongoQueue<string>("mongodb://127.0.0.1/queue_test", "queue_test", "collector_queue", 32000);

            while (true)
            {
                Console.Write("Waiting... <");
                Console.Out.Flush();

                string result = workerQueue.Receive();
                collectorQueue.Send(result.ToUpper());

                Console.WriteLine(result + ">");
            }
        }
    }
}
