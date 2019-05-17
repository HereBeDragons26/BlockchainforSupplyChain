using Blockchain.Model;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Blockchain {
    public class Block {
        public DateTime Time { get; set; }
        public long BlockID { get; set; }
        public Data Data { get; set; }
        public string Hash { get; set; }
        public string PreviousHash { get; set; }
        public int Nonce { get; set; }

        public Block(long blockID, Data data, string previousHash) {
            BlockID = blockID;
            Data = data;
            Hash = CalculateHash();
            PreviousHash = previousHash;
            Time = DateTime.MaxValue;
        }

        public void Mine() {
            int division = int.MaxValue / Miners.minerIPs.Count;
            Nonce = 0;
            for (int a = 0; a < Miners.minerIPs.Count; a++) {
                if (Miners.minerIPs[a].Equals(TCP.myIP))
                    Nonce = division * a;
            }
            Console.WriteLine("Mine started");
            while (true) {
                //if(Nonce%1000 == 0)
                //    Console.WriteLine(Nonce);
                if (ChangeNonce(Nonce)) {
                    Console.WriteLine("Mine ended");
                    Time = DateTime.Now;
                    Miners.SetMyMinerTrue(this);
                    TCP.SendAllMiners("checkNonce"+ Time.ToString() + "$" + BlockID.ToString() + "$" + Nonce.ToString());
                    break;
                }
                else
                    Nonce++;
            }
            Console.WriteLine(
                "Block's hash is -> " + Hash +
                "\nBlock's nonce is -> " + Nonce + "\n" +
                "Found Date is -> " + Time + "\n");
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
            byte[] input = Encoding.ASCII.GetBytes(BlockID.ToString() + Data.ToString() + Nonce.ToString());
            byte[] output = sHA256.ComputeHash(input);
            return Convert.ToBase64String(output);
        }

        public override string ToString() {
            return
                "\n---Block---" + 
                "\nBlockID: " + BlockID +
                "\nTime: " + Time +
                "\nHash: " + Hash +
                "\nNonce: " + Nonce +
                "\n---Block---\n";
        }
    }
}
