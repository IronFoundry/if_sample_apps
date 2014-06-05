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
            MongoQueue<string> collectorQueue = new MongoQueue<string>("mongodb://127.0.0.1/queue_test", "queue_test", "collector_queue", 32000);
            server = new WebSocketServer();
            bool successfulSetup = server.Setup(59567);
            server.NewMessageReceived += server_NewMessageReceived;
            server.NewSessionConnected += server_NewSessionConnected;
            bool successfulStart = server.Start();

            while (true)
            {
                Console.Write("Waiting for data...<");
                Console.Out.Flush();

                string userData = collectorQueue.Receive();

                Console.Write(userData + "> writing to client...");
                Console.Out.Flush();

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
                Console.Write("\t" + session.RemoteEndPoint.Address + ":" + session.RemoteEndPoint.Port + "...");
                Console.Out.Flush();

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
