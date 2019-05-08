using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Blockchain {
    public class BlockChain {
        
        private static List<Block> Chain = null;
        private static readonly long genesisBlockID = 12589;
        public static string beginningOfHash = "000";
        private static List<List<KeyValuePair<Block, bool>>> miners;
        public static List<string> minerIPs;
        public static string myIP;
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


        public static void AddNewMiner(string ip) {
            miners.Add(null);
            minerIPs.Add(ip);
            Console.WriteLine("New miner added: " + ip);
        }

        public static void ReceiveNewBlock(Data data) {
            Block block = null;
            for (int a = 0; a < miners.Count; a++) {
                block = CreateBlock(data);
                miners[a].Add(new KeyValuePair<Block, bool>(block, false));
            }
            Console.WriteLine("Received new block");
            nonceFinderThread = new Thread(new ThreadStart(block.Mine)) {
                Name = "NonceFinderThread"
            };
            nonceFinderThread.Start();
        }

        public static void AddBlockToChain(Block block) {
            for(int a = 0; a < miners.Count; a++) {
                for(int b = 0; b < miners[a].Count; a++) {
                    if (miners[a][b].Key == block) {
                        miners[a].RemoveAt(b);
                        Chain.Add(block);
                        break;
                    }
                }
            }
            Console.WriteLine("Block added to chain.");
        }

        public static void CheckNonceIsTrue(DateTime time, Block block, int nonce) {
            if (block.ChangeNonce(nonce)) {
                for (int a = 0; a < miners.Count; a++) {
                    for (int b = 0; b < miners[a].Count; a++) {
                        if (miners[a][b].Key == block) {
                            miners[a].Add(new KeyValuePair<Block, bool>(block, true));
                            TryToAddChain(time, block);
                        }
                    }
                }
            }
            else Console.WriteLine("Nonce is wrong");
        }

        public static void TryToAddChain(DateTime time, Block block) {
            int countOfTrueBlock = 0;
            for (int a = 0; a < miners.Count; a++) {
                for (int b = 0; b < miners[a].Count; a++) {
                    if (miners[a][b].Key == block) {
                        if (miners[a][b].Value) {
                            countOfTrueBlock++;
                            break;
                        }
                    }
                }
            }
            Console.WriteLine("Miners Count: " + miners[0].Count);
            Console.WriteLine("True Count: " + countOfTrueBlock);
            if (countOfTrueBlock > miners[0].Count / 2) {
                block.Time = time;
                AddBlockToChain(block);
            }
        }
    }
}
