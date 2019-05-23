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
        public static void SetMyMinerTrue(Block block) {
            UpdateProcessingBlockList(block);
        }

        /// <summary>
        /// Set miner true when another miner found the nonce
        /// </summary>
        /// <param name="time"></param>
        /// <param name="blockID"></param>
        /// <param name="nonce"></param>
        public static void SetMyMinerTrue(DateTime time, long blockID, int nonce, string ip) {
            Console.WriteLine("Nonce is checking for " + blockID);
            var keyValuePair = GetBlockInProcessingBlock(blockID);
            Block block = keyValuePair.Value;

            if (block == null) return;

            if (block.ChangeNonce(nonce)) {
                block.Time = time;
                Console.WriteLine("Nonce is true for " + blockID);

                UpdateProcessingBlockList(block);
                SetMinersTrue(ip, block.BlockID);

                TCP.SendAllMiners("nonceIsTrue" + blockID);
            }
            else Console.WriteLine("Nonce is wrong for " + blockID);
        }

        public static void SetMinersTrue(string ip, long blockID) {
            int minerIndex = FindIndexOfGivenInput(ip);
            var keyValuePair = GetBlockInProcessingBlock(blockID);
            int blockIndex = keyValuePair.Key;
            Block block = keyValuePair.Value;

            if (block == null) return;

            if (miners[minerIndex][blockIndex].Key.BlockID == block.BlockID) {
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

        public static void UpdateProcessingBlockList(Block block) {
            int myIndex = FindIndexOfGivenInput(TCP.myIP);
            var keyValuePair = GetBlockInProcessingBlock(block.BlockID);
            int blockIndex = keyValuePair.Key;
            Block processingBlock = keyValuePair.Value;

            if (processingBlock == null) return;

            if (miners[myIndex][blockIndex].Key.Time.CompareTo(block.Time) >= 0) {
                miners[myIndex].RemoveAt(blockIndex);
                processingBlock.Time = block.Time;
                processingBlock.Nonce = block.Nonce;
                processingBlock.Hash = block.Hash;
                miners[myIndex].Add(new KeyValuePair<Block, bool>(processingBlock, true));
            }
            BlockChain.TryToAddChain(processingBlock);
        }
    }
}
