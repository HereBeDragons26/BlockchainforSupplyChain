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
        public static string WebServerIp;
        public static string myIP;

        public static void Send(string ip, string message) {
            try {
                TcpClient tcpClient = new TcpClient(ip, MinerPort);
                NetworkStream stream = tcpClient.GetStream();

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(message);

                stream.Write(ba, 0, ba.Length);

                tcpClient.Close();
                stream.Flush();
                stream.Close();
            }
            catch (Exception e) {
                Console.WriteLine("----------Can't connect to miner----------\n\n" + e.StackTrace);
            }
        }

        public static void SendWebServer(string message) {
            try {
                TcpClient tcpClient = new TcpClient(WebServerIp, WebServerPort);
                NetworkStream stream = tcpClient.GetStream();

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(message);

                stream.Write(ba, 0, ba.Length);

                tcpClient.Close();
                stream.Flush();
                stream.Close();
            }
            catch(Exception e) {
                Console.WriteLine("----------Can't connect to webserver----------\n\n" + e.StackTrace);
            }
        }


        public static void SendAllMiners(string message) {
            try {
                Miners.minerIPs.ForEach(mp => {

                    if (mp == myIP) return;

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
            catch (Exception e) {
                Console.WriteLine("----------Can't connect to miners----------\n\n" + e.StackTrace);
            }
        }

        private static void ListenerMethod() {
            TcpListener listener = new TcpListener(IPAddress.Any, MinerPort);
            listener.Start();
            Socket client = null;

            while (true) {
                try {
                    client = listener.AcceptSocket();
                    string ip = ((IPEndPoint)client.RemoteEndPoint).Address.ToString();
                    byte[] bytes = new byte[1024];

                    int size = client.Receive(bytes);
                    string str = "";
                    for (int i = 0; i < size; i++)
                        str += Convert.ToChar(bytes[i]);

                    new Thread(() => Interpreter(str, ip)).Start();

                }
                catch (Exception e) {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        public static void StartListener() {
            listenerThread = new Thread(new ThreadStart(ListenerMethod)) {
                Name = "TCPConnection.ListenThread"
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
                Miners.minerIPs = obj.miners;
                Console.WriteLine("Current miner count: " + Miners.minerIPs.Count);

                for (int a = 0; a < Miners.minerIPs.Count; a++) {
                    Miners.miners.Add(new List<KeyValuePair<Block, bool>>());
                }

                if(Miners.minerIPs.Count != 0)
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
                Miners.SetMyMinerTrue(DateTime.Parse(checkNonceArray[0]), long.Parse(checkNonceArray[1]), Int32.Parse(checkNonceArray[2]), ip);
                return;
            }

            if (message.StartsWith("nonceIsTrue")) {
                Console.WriteLine("nonceIsTrue");
                message = message.Substring(11);
                Miners.SetMinersTrue(ip, long.Parse(message));
                return;
            }

            if (message.StartsWith("getChain")) {
                Send(ip, "receiveChain" + JsonSerialize(new { chain = BlockChain.GetChain() }));
                return;
            }

            if (message.StartsWith("receiveChain")) {
                object ret = JsonDeserialize(message.Substring(12));
                var obj = Cast(ret, new { chain = new List<Block>() });
                BlockChain.ReceiveChain(obj.chain);
                return;
            }

            if (message.StartsWith("newMinerJoined")) {
                message = message.Substring(14);
                Miners.minerIPs.Add(message);
                Miners.miners.Add(new List<KeyValuePair<Block, bool>>());
                if(message == myIP) {
                    Console.WriteLine("\n-------Connected to network!-------\n");
                }
                else {
                    Console.WriteLine("New miner joined to network -> " + message);
                }
                Console.WriteLine("Current miner count: " + Miners.minerIPs.Count);
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

            if (message.StartsWith("block")) {
                message = message.Substring(5);
                Block takenBlock = (Block) JsonDeserialize(message);

                // wait until the block is inserted into chain
                while (!BlockChain.GetChain().Exists( b => b.BlockID == takenBlock.BlockID)) ;

                Console.WriteLine("block recieved from a miner");

                Block currentBlock = BlockChain.GetBlock(takenBlock.BlockID);

                // change time earlier if taken one is earlier
                if (currentBlock.Time.CompareTo(takenBlock.Time) > 0) currentBlock.Time = takenBlock.Time;

                // change smaller nonce if the found dates equal
                if (currentBlock.Time.CompareTo(takenBlock.Time) == 0) currentBlock.Nonce = Math.Min(takenBlock.Nonce, currentBlock.Nonce);

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
