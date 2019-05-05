using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Blockchain {
    public class Block {
        public DateTime Time { get; set; }
        public List<long> ParentID { get; set; }
        public long BlockID { get; set; }
        public Product Product { get; set; }
        public string Hash { get; set; }
        public string PreviousHash { get; set; }

        public Block(List<long> parentID, long blockID, Product product, string previousHash) {
            Time = DateTime.Now;
            ParentID = parentID;
            BlockID = blockID;
            Product = product;
            Hash = CalculateHash();
            PreviousHash = previousHash;
        }

        /// <summary>
        /// Calculate block's hash with Time, ParantID, BlockID and Product object
        /// </summary>
        /// <returns>Hash of block</returns>
        public string CalculateHash() {
            SHA256 sHA256 = SHA256.Create();
            byte[] input = Encoding.ASCII.GetBytes(Time + ParentID.ToString() + BlockID + Product);
            byte[] output = sHA256.ComputeHash(input);
            return Convert.ToString(output);
        }
    }
}
