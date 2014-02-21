﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vtortola.WebSockets;

namespace WebSockets.TestConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            CancellationTokenSource cancellation = new CancellationTokenSource();
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8001);
            WebSocketListener server = new WebSocketListener(endpoint);

            server.Start();
            Log("Server started at " + endpoint.ToString());

            var task = AcceptWebSocketClients(server, cancellation.Token);

            Console.ReadKey(true);
            Log("Server stoping"); 
            cancellation.Cancel();
            Console.ReadKey(true);
        }


        static async Task AcceptWebSocketClients(WebSocketListener server, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var ws = await server.AcceptWebSocketClientAsync();
                Log("Client Connected: " + ws.RemoteEndpoint.ToString());

                Task.Run(async () =>
                {
                    try
                    {
                        while (ws.IsConnected && !token.IsCancellationRequested)
                        {
                            var msg = await ws.ReadAsync();
                            if (msg != null)
                            {
                                Log("Client says: " + msg.Length);
                                await ws.WriteAsync(new String(msg.Reverse().ToArray()));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("Error : " + ex.Message);
                    }
                    Log("Client Disconnected: " + ws.RemoteEndpoint.ToString());
                }, token);

                //Task.Run(async () =>
                //{
                //    try
                //    {
                //        while (ws.IsConnected && !token.IsCancellationRequested)
                //        {
                //            Byte[] buffer = new Byte[4096];

                //            var state = await ws.ReadAsync(buffer, 0, buffer.Length);
                //            if (state != null)
                //            {
                //                Log("Client says: " + state.BytesReaded);
                //                await ws.WriteAsync(buffer, 0, state.BytesReaded, true, WebSocketMessageType.ByteArray);
                //            }
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        Log("Error : " + ex.Message);
                //    }
                //    Log("Client Disconnected: " + ws.RemoteEndpoint.ToString());
                //}, token);
            }
        }

        static void Log(String line)
        {
            Console.WriteLine(DateTime.Now.ToString("dd/MM/yyy hh:mm:ss.fff ") + line);
        }

    }
}