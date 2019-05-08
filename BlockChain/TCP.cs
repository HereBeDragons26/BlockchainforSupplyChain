using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Blockchain {
    public class TCP {

        static Thread listenerThread = null;

        public static readonly int PORT = 13000;
        public static string SenderIP = "10.27.49.8";
        public static string WebServerIp;
        //public static int ListenerPort { get; set; }

        public static void SendWebServer(string message) {
            TcpClient tcpClient = new TcpClient(WebServerIp, PORT);
            NetworkStream stream = tcpClient.GetStream();

            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(message);

            stream.Write(ba, 0, ba.Length);

            tcpClient.Close();
            stream.Flush();
            stream.Close();
        }

        public static void SendAllMiners(string message) {

            BlockChain.minerIPs.ForEach(mp => {

                if (mp == BlockChain.myIP) return;

                TcpClient tcpClient = new TcpClient(mp, PORT);
                NetworkStream stream = tcpClient.GetStream();

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(message);

                stream.Write(ba, 0, ba.Length);

                tcpClient.Close();
                stream.Flush();
                stream.Close();

            });

        }

        private static void listenerMethod() {
            while (true) {
                TcpListener listener = new TcpListener(IPAddress.Any, PORT);
                listener.Start();
                Socket client = null;
                try {
                    client = listener.AcceptSocket();
                    String ip = ((IPEndPoint)client.RemoteEndPoint).Address.ToString();
                    byte[] bytes = new byte[1024];

                    int size = client.Receive(bytes);
                    string str = "";
                    for (int i = 0; i < size; i++)
                        str += Convert.ToChar(bytes[i]);

                    new Thread(() => Interpreter(str, ip)).Start();

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

        public static void StartListener() {
            listenerThread = new Thread(new ThreadStart(listenerMethod)) {
                Name = "UdpConnection.ListenThread"
            };
            listenerThread.Start();
        }

        private static void Interpreter(string message, string ip) {

            // received miners list
            if (message.StartsWith("minersList")) {

                

                return;
            }

            // received a new block to add
            if (message.StartsWith("addBlock")) {
                Data data = (Data)JsonDeserialize(message.Substring(8));
                BlockChain.ReceiveNewBlock(data);
                return;
            }

            if (message.StartsWith("checkNonce")) {
                message = message.Substring(10);
                string[] checkNonceArray = message.Split('$');
                BlockChain.SetMyMinerTrue(DateTime.Parse(checkNonceArray[0]), long.Parse(checkNonceArray[1]), Int32.Parse(checkNonceArray[2]));
                return;
            }

            if (message.StartsWith("nonceIsTrue")) {
                message = message.Substring(11);
                BlockChain.SetMinersTrue(ip, long.Parse(message));
                return;
            }

        }

        public static string JsonSerialize(object graph) {
            return JsonConvert.SerializeObject(graph,
                Formatting.None,
                new JsonSerializerSettings {
                    TypeNameHandling = TypeNameHandling.Objects
                });
        }

        public static object JsonDeserialize(string seralized) {
            return JsonConvert.DeserializeObject(seralized,
                new JsonSerializerSettings {
                    TypeNameHandling = TypeNameHandling.Objects
                });
        }

    }
}
