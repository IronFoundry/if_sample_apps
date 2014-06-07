using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using MongoLib;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine.Configuration;
using SuperWebSocket;

namespace Collector
{
    public class CollectorApp
    {
        private static WebSocketServer server;
        private static List<WebSocketSession> sessions = new List<WebSocketSession>();
 
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("******* Collector starting ********");

            CloudFoundryMongoBinder binder = new CloudFoundryMongoBinder();
            MongoQueue<string> collectorQueue = new MongoQueue<string>(binder.Url, binder.DatabaseName, "collector_queue", 32000);
            server = new WebSocketServer();
            
            if(binder.AppPort.HasValue == false) throw new ArgumentNullException("port", "Is not set!");

            bool successfulSetup = server.Setup(Convert.ToInt32(binder.AppPort.Value));
            Console.WriteLine("Server setup complete");
            server.NewMessageReceived += server_NewMessageReceived;
            server.NewSessionConnected += server_NewSessionConnected;
            bool successfulStart = server.Start();
            Console.WriteLine("Server started");

            while (true)
            {
                Console.WriteLine("Waiting for data...<");

                string userData = collectorQueue.Receive();

                Console.WriteLine(userData + "> writing to client...");

                SendToClient(userData);

                Console.WriteLine("data written");
            }
        }

        static void server_NewSessionConnected(WebSocketSession session)
        {
            Console.WriteLine("Adding new session from " + session.RemoteEndPoint);
            sessions.Add(session);
        }

        private static void SendToClient(string userData)
        {
            Console.WriteLine("Sending data to client");
            foreach (var session in server.GetSessions(c => true))
            {
                Console.WriteLine(session.RemoteEndPoint.Address + ":" + session.RemoteEndPoint.Port + "...");

                session.Send(userData);

                Console.WriteLine("Sent");
            }
        }

        private static void server_NewMessageReceived(WebSocketSession session, string value)
        {
            Console.WriteLine("Message just received: " + value);
        }
    }
}
