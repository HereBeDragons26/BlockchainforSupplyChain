using Blockchain.Model;
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

        public static readonly int WebServerPort = 13001;
        public static readonly int MinerPort = 13000;
        public static string WebServerIp = "192.168.43.14";
        public static string myIP;

        public static void SendWebServer(string message) {
            TcpClient tcpClient = new TcpClient(WebServerIp, WebServerPort);
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

                if (mp == TCP.myIP) return;

                TcpClient tcpClient = new TcpClient(mp, MinerPort);
                NetworkStream stream = tcpClient.GetStream();

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(message);

                stream.Write(ba, 0, ba.Length);

                tcpClient.Close();
                stream.Flush();
                stream.Close();
            });
        }

        private static void ListenerMethod() {

            TcpListener listener = new TcpListener(IPAddress.Any, MinerPort);
            listener.Start();
            Socket client = null;

            while (true) {
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
            }
        }

        public static void StartListener() {
            listenerThread = new Thread(new ThreadStart(ListenerMethod)) {
                Name = "UdpConnection.ListenThread"
            };
            listenerThread.Start();
        }

        private static void Interpreter(string message, string ip) {

            message = message.Replace("SupplyChain", "Blockchain");
            // received miners list
            if (message.StartsWith("minersList")) {
                message = message.Substring(10);
                //Console.WriteLine(BlockChain.minerIPs.Count);
                object ret = JsonDeserialize(message);
                var obj = Cast(ret, new { miners = new List<string>() });
                BlockChain.minerIPs = obj.miners;
                Console.WriteLine("Current miner count: " + BlockChain.minerIPs.Count);

                for (int a = 0; a < BlockChain.minerIPs.Count; a++) {
                    BlockChain.miners.Add(new List<KeyValuePair<Block, bool>>());
                }

                if(BlockChain.minerIPs.Count != 0)
                    SendAllMiners("getChain");
                else {
                    BlockChain.GetChain();
                    SendWebServer("addMeNow");
                }
                return;
            }

            // received a new block to add
            if (message.StartsWith("addBlock")) {
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
                //Send(ip, )
                return;
            }

            if(message.StartsWith("newMinerJoined")) {
                message = message.Substring(14);
                BlockChain.minerIPs.Add(message);
                BlockChain.miners.Add(new List<KeyValuePair<Block, bool>>());
                if(message == myIP) {
                    Console.WriteLine("\n-------Connected to network!-------\n");
                }
                else {
                    Console.WriteLine("New miner joined to network -> " + message);
                }
                Console.WriteLine("Current miner count: " + BlockChain.minerIPs.Count);
                return;
            }

            if (message.StartsWith("verify")) {
                Console.WriteLine("verify");
                message = message.Substring(6);
                Product product = BlockChain.GetProductInfo(long.Parse(message));
                SendWebServer("verifyReturn" + JsonSerialize(product));
                Console.WriteLine("Product returned");
                return;
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
