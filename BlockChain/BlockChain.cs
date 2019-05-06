using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain {
    public class BlockChain {
        
        private static List<Block> Chain = null;
        private static readonly long genesisBlockID = 12589;
        public static string beginningOfHash = "000";

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
            Chain.Add(new Block(new List<long>(), genesisBlockID, new Product(), "0"));
        }

        /// <summary>
        /// Creates block and adds it to chain
        /// </summary>
        /// <param name="parentID">Parent ID required to find this product's origin</param>
        /// <param name="product">List of features</param>
        public static void CreateBlock(List<long> parentID, Product product) {
            List<Block> chain = GetChain();
            Block lastBlock = chain[chain.Count - 1];
            string lastBlockHash = lastBlock.Hash;
            long lastBlockID = lastBlock.BlockID + 1;
            Chain.Add(new Block(parentID, lastBlockID, product, lastBlockHash));
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
            for (int a = 0; a < block.ParentID.Count; a++) {
                Block parentBlock = GetBlock(block.ParentID[a]);
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

        public static Product GetProductInfo(List<Block> blocks) {
            Product product = new Product();
            for (int a = 0; a < blocks.Count; a++) {
                for (int b = 0; b < blocks[a].Product.Features.Count; b++) {
                    product.Features.Add(blocks[a].Product.Features[b]);
                }
            }
            return product;
        }
    }
}
