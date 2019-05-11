using Blockchain.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Blockchain {
    public class BlockChain {
        
        private static List<Block> Chain = null;
        private static readonly long genesisBlockID = 12589;
        public static string beginningOfHash = "000";
        public static List<List<KeyValuePair<Block, bool>>> miners = new List<List<KeyValuePair<Block, bool>>>();
        public static List<string> minerIPs = null;
        public static string myIP = "10.27.49.6";
        private static Thread nonceFinderThread = null;

        private BlockChain() {
            GetChain();
        }
        public static List<Block> GetChain() {
            if (Chain == null) {
                Chain = new List<Block>();
                CreateGenesisBlock();
            }
            return Chain;
        }

        /// <summary>
        /// Creates first block of chain
        /// </summary>
        public static void CreateGenesisBlock() {
            Chain.Add(new Block(genesisBlockID, new Data(), "0"));
        }

        /// <summary>
        /// Creates block and adds it to chain
        /// </summary>
        /// <param name="data">List of features and parent IDs</param>
        public static Block CreateBlock(Data data) {
            List<Block> chain = GetChain();
            Block lastBlock = chain[chain.Count - 1];
            string lastBlockHash = lastBlock.Hash;
            long lastBlockID = lastBlock.BlockID + 1;
            return new Block(lastBlockID, data, lastBlockHash);
        }

        /// <summary>
        /// Get block from chain
        /// </summary>
        /// <param name="blockID">Block's ID</param>
        /// <returns></returns>
        public static Block GetBlock(long blockID) {
            return Chain[(int)(blockID - genesisBlockID)];
        }

        public static List<Block> productBlocks;
        private static void GetAllBlockRec(Block block) {
            productBlocks.Add(block);
            for (int a = 0; a < block.Data.ParentID.Count; a++) {
                Block parentBlock = GetBlock(block.Data.ParentID[a]);
                GetAllBlockRec(parentBlock);
            }
        }

        /// <summary>
        /// Gets block with given ID and all its parent blocks
        /// </summary>
        /// <param name="blockID">Block's ID</param>
        /// <returns>List of blocks</returns>
        public static List<Block> GetAllBlock(long blockID) {
            GetAllBlockRec(GetBlock(blockID));
            return productBlocks;
        }

        /// <summary>
        /// Get all features which given product
        /// </summary>
        /// <param name="blockID"></param>
        /// <returns>List of features</returns>
        public static Product GetProductInfo(long blockID) {
            List<Block> blocks = GetAllBlock(blockID);
            Product product = new Product();
            for (int a = 0; a < blocks.Count; a++) {
                for (int b = 0; b < blocks[a].Data.Product.Features.Count; b++) {
                    product.Features.Add(blocks[a].Data.Product.Features[b]);
                }
            }
            return product;
        }

        public static void ReceiveNewBlock(Data data) {
            Block block = null;
            for (int a = 0; a < miners.Count; a++) {
                block = CreateBlock(data);
                miners[a].Add(new KeyValuePair<Block, bool>(block, false));
            }
            Console.WriteLine("Received new block");
            nonceFinderThread = new Thread(new ThreadStart(block.Mine)) {
                Name = "NonceFinderThread" + block.BlockID
            };
            nonceFinderThread.Start();
        }

        public static void AddBlockToChain(Block block, long blockID) {
            for(int a = 0; a < miners.Count; a++) {
                for(int b = 0; b < miners[a].Count; b++) {
                    if (miners[a][b].Key.BlockID == blockID) {
                        miners[a].RemoveAt(b);
                        break;
                    }
                }
            }
            Chain.Add(block);
            Console.WriteLine("Block added to chain.");
        }

        public static void SetMyMinerTrue(DateTime time, long blockID) {
            Block block = null;
            for (int a = 0; a < miners[0].Count; a++) {
                if (miners[0][a].Key.BlockID == blockID) {
                    block = miners[0][a].Key;
                    break;
                }
            }
            int minerIndex = 0;
            for(int a = 0; a < minerIPs.Count; a++) {
                if(minerIPs[a].Equals(myIP)) {
                    minerIndex = a;
                    break;
                }
            }
            for (int a = 0; a < miners[minerIndex].Count; a++) {
                if (miners[minerIndex][a].Key == block) {
                    miners[minerIndex].RemoveAt(a);
                    block.Time = time;
                    miners[minerIndex].Add(new KeyValuePair<Block, bool>(block, true));
                    TryToAddChain(block);
                }
            }
        }

        public static void SetMyMinerTrue(DateTime time, long blockID, int nonce) {
            Block block = null;
            for (int a = 0; a < miners[0].Count; a++) {
                if (miners[0][a].Key.BlockID == blockID)
                    block = miners[0][a].Key;
            }
            if (block.ChangeNonce(nonce)) {
                int minerIndex = 0;
                for (int a = 0; a < minerIPs.Count; a++) {
                    if (minerIPs[a].Equals(myIP)) {
                        minerIndex = a;
                        break;
                    }
                }
                for (int a = 0; a < miners[minerIndex].Count; a++) {
                    if (miners[minerIndex][a].Key == block) {
                        block.Time = time;
                        miners[minerIndex].Add(new KeyValuePair<Block, bool>(block, true));
                        TryToAddChain(block);
                    }
                }
                TCP.SendAllMiners("nonceIsTrue" + blockID);
            }
            else Console.WriteLine("Nonce is wrong");
        }

        public static void SetMinersTrue(string ip, long blockID) {
            Block block = null;
            for (int a = 0; a < miners[0].Count; a++) {
                if (miners[0][a].Key.BlockID == blockID)
                    block = miners[0][a].Key;
            }

            int minerIndex = 0;
            for (int a = 0; a < minerIPs.Count; a++) {
                if (minerIPs[a].Equals(ip)) {
                    minerIndex = a;
                    break;
                }
            }
            for (int a = 0; a < miners[minerIndex].Count; a++) {
                if (miners[minerIndex][a].Key == block) {
                    miners[minerIndex].Add(new KeyValuePair<Block, bool>(block, true));
                    TryToAddChain(block);
                }
            }
        }

        public static void TryToAddChain(Block block) {
            int countOfTrueBlock = 0;
            for (int a = 0; a < miners.Count; a++) {
                for (int b = 0; b < miners[a].Count; b++) {
                    if (miners[a][b].Key == block) {
                        if (miners[a][b].Value) {
                            countOfTrueBlock++;
                            break;
                        }
                    }
                }
            }
            Console.WriteLine("Miners Count: " + minerIPs.Count);
            Console.WriteLine("True Count: " + countOfTrueBlock);
            if (countOfTrueBlock > (minerIPs.Count / 2.0)) {
                AddBlockToChain(block, block.BlockID);
            }
        }

        public static void ConnectToNetwork() {
            TCP.SendWebServer("connectToNetwork" + myIP);
            Console.WriteLine("Connected to network!");
        }
    }
}
