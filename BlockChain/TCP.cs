﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Blockchain {
    public class TCP {

        static Thread listenerThread = null;

        public static readonly int PORT = 13000;
        public static string SenderIP = "10.31.55.37";
        public static string ReceiverIP = "10.31.55.49";
        //public static int ListenerPort { get; set; }

        public static void Send(string message) {
            TcpClient tcpClient = new TcpClient(ReceiverIP, PORT);
            NetworkStream stream = tcpClient.GetStream();

            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(message);

            stream.Write(ba, 0, ba.Length);

            tcpClient.Close();
            stream.Flush();
            stream.Close();
        }

        private static void listenerMethod() {
            while (true) {
                TcpListener listener = new TcpListener(IPAddress.Parse(SenderIP), PORT);
                listener.Start();
                Socket client = null;
                try {
                    client = listener.AcceptSocket();
                    byte[] bytes = new byte[1024];

                    int size = client.Receive(bytes);
                    string str = "";
                    for (int i = 0; i < size; i++)
                        str += Convert.ToChar(bytes[i]);

                    if (str.StartsWith("BattleshipGame")) {
                        str = str.Substring(14, str.Length - 14);
                    }
                }
                catch (Exception e) {
                    Console.WriteLine(e.ToString());
                }
                finally {
                    if (client != null)
                        client.Close();
                    if (listener != null)
                        listener.Stop();
                }
            }
        }

        private static void executer(String message, byte[] bytes) {
            //if (message.StartsWith("FileSize")) {
            //    message = message.Substring(8, message.Length - 8);
            //    String[] strArray = message.Split('$');
            //    Data.PackageSize = int.Parse(strArray[0]);
            //    Data.Image = new byte[int.Parse(strArray[1])];
            //    Data.ByteMax = int.Parse(strArray[1]);
            //    Receiver.connection(Data.SendIp);
        }

        public static void StartListener() {
            listenerThread = new Thread(new ThreadStart(listenerMethod)) {
                Name = "UdpConnection.ListenThread"
            };
            listenerThread.Start();
        }
    }
}