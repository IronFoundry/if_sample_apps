using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebListener
{
    public class WebsocketServer
    {
        public async void Start(String httpListenerPrefix)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(httpListenerPrefix);
            try
            {
                listener.Start();
            }
            catch (HttpListenerException httpListenerException)
            {
                Console.WriteLine("Error trying to start listening, error code is: " + httpListenerException.ErrorCode);
                Console.WriteLine("native error code: " + httpListenerException.NativeErrorCode + ", hresult: " + httpListenerException.HResult);
                return;
            }
            Console.WriteLine("Listening at " + httpListenerPrefix);

            while (true)
            {
                try
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    if (context.Request.IsWebSocketRequest)
                    {
                        ProcessRequest(context);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed in GetContextAsync" + e);
                    break;
                }
            }
        }

        private async void ProcessRequest(HttpListenerContext context)
        {
            WebSocketContext webSocketContext = null;
            try
            {
                webSocketContext = await context.AcceptWebSocketAsync(subProtocol: null);
                string ipAddress = context.Request.RemoteEndPoint.Address.ToString();
                Console.WriteLine("Connected to : " + ipAddress);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
                context.Response.StatusCode = 500;
                context.Response.Close();
                return;
            }

            WebSocket webSocket = webSocketContext.WebSocket;
            try
            {
                byte[] receiveBuffer = new byte[1024];
                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult receiveResult =
                        await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    }
                    else
                    {
                        await
                            webSocket.SendAsync(new ArraySegment<byte>(receiveBuffer, 0, receiveResult.Count),
                                WebSocketMessageType.Binary, receiveResult.EndOfMessage, CancellationToken.None);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed while accepting and reading message: ", e);
            }
            finally
            {
                if(webSocket != null) webSocket.Dispose();
            }
        }

        static void Main(string[] args)
        {
            String port = Environment.GetEnvironmentVariable("VCAP_APP_PORT");
            String host = Environment.GetEnvironmentVariable("VCAP_APP_HOST");
            if (String.IsNullOrEmpty(port)) throw new Exception("Port not set");

            WebsocketServer server = new WebsocketServer();
            string listenerAddress = "http://*:" + port + "/Foo/";
            Console.WriteLine("Listening on " + listenerAddress);

            server.Start(listenerAddress);

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
