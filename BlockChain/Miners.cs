using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain {
    public class Miners {

        public static List<List<KeyValuePair<Block, bool>>> miners = new List<List<KeyValuePair<Block, bool>>>();
        public static List<string> minerIPs = null;

        /// <summary>
        /// Set miner true when i found the nonce
        /// </summary>
        /// <param name="time"></param>
        /// <param name="blockID"></param>
        public static void SetMyMinerTrue(DateTime time, long blockID) {
            Console.WriteLine("SetMyMinerTrue");
            UpdateProcessingBlockList(time, blockID);
        }

        /// <summary>
        /// Set miner true when another miner found the nonce
        /// </summary>
        /// <param name="time"></param>
        /// <param name="blockID"></param>
        /// <param name="nonce"></param>
        public static void SetMyMinerTrue(DateTime time, long blockID, int nonce) {
            Console.WriteLine("SetMyMinerTrueWithNonce");
            Console.WriteLine("Nonce is checking for " + blockID);
            var keyValuePair = GetBlockInProcessingBlock(blockID);
            Block block = keyValuePair.Value;

            if (block.ChangeNonce(nonce)) {
                Console.WriteLine("Nonce is true for " + blockID);

                UpdateProcessingBlockList(time, blockID);
                TCP.SendAllMiners("nonceIsTrue" + blockID);
            }
            else Console.WriteLine("Nonce is wrong for " + blockID);
        }

        public static void SetMinersTrue(string ip, long blockID) {
            Console.WriteLine("SetMinersTrue");
            int minerIndex = FindIndexOfGivenInput(ip);
            var keyValuePair = GetBlockInProcessingBlock(blockID);
            int blockIndex = keyValuePair.Key;
            Block block = keyValuePair.Value;

            if (miners[minerIndex][blockIndex].Key == block) {
                miners[minerIndex].RemoveAt(blockIndex);
                miners[minerIndex].Add(new KeyValuePair<Block, bool>(block, true));
                BlockChain.TryToAddChain(block);
            }
        }


        public static void ConnectToNetwork() {
            TCP.SendWebServer("connectToNetwork" + TCP.myIP);
            Console.WriteLine("Connect request sent to network");
        }

        public static int FindIndexOfGivenInput(string input) {
            for (int a = 0; a < minerIPs.Count; a++) {
                if (minerIPs[a].Equals(input)) {
                    return a;
                }
            }
            return -1;
        }

        public static KeyValuePair<int, Block> GetBlockInProcessingBlock(long blockID) {
            int myIndex = FindIndexOfGivenInput(TCP.myIP);
            for (int a = 0; a < miners[myIndex].Count; a++) {
                if (miners[myIndex][a].Key.BlockID == blockID) {
                    return new KeyValuePair<int, Block>(a, miners[myIndex][a].Key);
                }
            }
            return new KeyValuePair<int, Block>();
        }

        public static void UpdateProcessingBlockList(DateTime time, long blockID) {
            int myIndex = FindIndexOfGivenInput(TCP.myIP);
            var keyValuePair = GetBlockInProcessingBlock(blockID);
            int blockIndex = keyValuePair.Key;
            Block block = keyValuePair.Value;

            if (!((miners[myIndex][blockIndex].Key.Time < time) && miners[myIndex][blockIndex].Value)) {
                miners[myIndex].RemoveAt(blockIndex);
                block.Time = time;
                miners[myIndex].Add(new KeyValuePair<Block, bool>(block, true));
            }
            BlockChain.TryToAddChain(block);
        }
    }
}
