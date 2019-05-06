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
        public int Nonce { get; set; }

        public Block(List<long> parentID, long blockID, Product product, string previousHash) {
            ParentID = parentID;
            BlockID = blockID;
            Product = product;
            Hash = CalculateHash();
            PreviousHash = previousHash;
        }

        public bool Mine() {
            Console.WriteLine("Mine start");
            Nonce = 0;
            while (true) {
                //if(Nonce%1000 == 0)
                //    Console.WriteLine(Nonce);
                if (ChangeNonce(Nonce)) {
                    //Console.WriteLine(Nonce + " Found");
                    //Nonce++;
                    Time = DateTime.Now;
                    return true;
                }
                else
                    Nonce++;
            }
        }

        public bool ChangeNonce(int nonce) {
            Nonce = nonce;
            Hash = CalculateHash();
            if (Hash.StartsWith(BlockChain.beginningOfHash)) return true;
            else return false;
        }

        /// <summary>
        /// Calculate block's hash with Time, ParantID, BlockID and Product object
        /// </summary>
        /// <returns>Hash of block</returns>
        public string CalculateHash() {
            SHA256 sHA256 = SHA256.Create();
            byte[] input = Encoding.ASCII.GetBytes(ParentID.ToString() + BlockID.ToString() + Product.ToString() + Nonce.ToString());
            byte[] output = sHA256.ComputeHash(input);
            return Convert.ToBase64String(output);
        }
    }
}
