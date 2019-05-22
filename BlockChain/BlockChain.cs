using Blockchain.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Blockchain {
    public class BlockChain {

        public static BlockChain blockChainInstance;

        private static List<Block> Chain = null;
        private static readonly long genesisBlockID = 12589;
        public static string beginningOfHash = "000";

        private static Thread nonceFinderThread = null;

        private BlockChain() {
            GetChain();
            blockChainInstance = this;
        }
        public static List<Block> GetChain() {
            if (Chain == null) {
                Chain = new List<Block>();
                CreateGenesisBlock();
            }
            return Chain;
        }

        public static void ReceiveChain(List<Block> blockChain) {
            Chain = blockChain;
            Console.WriteLine("Received blockchain");
            TCP.SendWebServer("addMeNow");
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

        public static List<Block> productBlocks = new List<Block>();
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

        /// <summary>
        /// Website sent new block
        /// </summary>
        /// <param name="data"></param>
        public static void ReceiveNewBlock(Data data) {
            Block block = null;
            for (int a = 0; a < Miners.miners.Count; a++) {
                block = CreateBlock(data);
                Miners.miners[a].Add(new KeyValuePair<Block, bool>(block, false));
            }
            Console.WriteLine("Received new block");
            nonceFinderThread = new Thread(new ThreadStart(block.Mine)) {
                Name = "NonceFinderThread" + block.BlockID
            };
            nonceFinderThread.Start();
        }

        public static void TryToAddChain(Block block) {
            Console.WriteLine("TryToAddChain block -> " + block.Time + " " + block.Nonce);
            int countOfTrueBlock = 0;
            for (int a = 0; a < Miners.miners.Count; a++) {
                for (int b = 0; b < Miners.miners[a].Count; b++) {
                    if (Miners.miners[a][b].Key.BlockID == block.BlockID) {
                        if (Miners.miners[a][b].Value) {
                            countOfTrueBlock++;
                            break;
                        }
                    }
                }
            }
            Console.WriteLine("Miners Count: " + Miners.minerIPs.Count);
            Console.WriteLine("True Count: " + countOfTrueBlock);
            if (countOfTrueBlock > (Miners.minerIPs.Count / 2.0)) {
                AddBlockToChain(block);
            }
        }

        /// <summary>
        /// Adds block to chain if more than %50 of miners agrees it
        /// </summary>
        /// <param name="block"></param>
        /// <param name="blockID"></param>
        public static void AddBlockToChain(Block block) {

            for (int a = 0; a < Miners.miners.Count; a++) {
                for (int b = 0; b < Miners.miners[a].Count; b++) {
                    if (Miners.miners[a][b].Key.BlockID == block.BlockID) {
                        if(Miners.miners[a][b].Key.Time.CompareTo(block.Time) < 0) {
                            block.Time = Miners.miners[a][b].Key.Time;
                            block.Nonce = Miners.miners[a][b].Key.Nonce;
                            block.Hash = Miners.miners[a][b].Key.Hash;
                        }
                        Miners.miners[a].RemoveAt(b);
                        break;
                    }
                }
            }

            for(int a = 0; a < Chain.Count; a++) {
                if(Chain[a].BlockID == block.BlockID) {
                    return;
                }
            }

            Chain.Add(block);

            // notify all miners that a block is added into the local chain
            TCP.SendAllMiners("block" + TCP.JsonSerialize(block));

        }

    }
}
