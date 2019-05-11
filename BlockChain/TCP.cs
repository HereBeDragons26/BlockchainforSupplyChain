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
        public static string WebServerIp = "10.27.19.162";

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
                message = message.Substring(10);
                var tempType = new {
                    miners = new List<string>()
                };

                Object ret = JsonDeserialize(message);
                var obj = Cast(ret, new { miners = new List<string>() });

                for (int a = 0; a < BlockChain.minerIPs.Count; a++) {
                    BlockChain.miners.Add(new List<KeyValuePair<Block, bool>>());
                }
                //TCP.SendAllMiners("getChain");
                TCP.SendWebServer("addMeNow");
                return;
            }

            // received a new block to add
            if (message.StartsWith("addBlock")) {
                Console.WriteLine("addBlock");
                Data data = (Data)JsonDeserialize(message.Substring(8));
                BlockChain.ReceiveNewBlock(data);
                return;
            }

            if (message.StartsWith("checkNonce")) {
                Console.WriteLine("checkNonce");
                message = message.Substring(10);
                string[] checkNonceArray = message.Split('$');
                BlockChain.SetMyMinerTrue(DateTime.Parse(checkNonceArray[0]), long.Parse(checkNonceArray[1]), Int32.Parse(checkNonceArray[2]));
                return;
            }

            if (message.StartsWith("nonceIsTrue")) {
                Console.WriteLine("nonceIsTrue");
                message = message.Substring(11);
                BlockChain.SetMinersTrue(ip, long.Parse(message));
                return;
            }

            if (message.StartsWith("getChain")) {
                message = message.Substring(8);
                //SendWebServer("addMeNow");
                return;
            }

            if(message.StartsWith("newMinerJoined")) {
                BlockChain.minerIPs.Add(ip);
                BlockChain.miners.Add(new List<KeyValuePair<Block, bool>>());
            }

        }

        public static T Cast<T>(object obj, T type) { return (T)obj; }

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
