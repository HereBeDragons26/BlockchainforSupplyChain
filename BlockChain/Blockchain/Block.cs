using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Blockchain
{
   public class Block
    {
        private DateTime Times { get; set; }
        private int ProductID { get; set; }
        private int CompanyID { get; set; }
        private string Data { get; set; }
        private string Hash { get; set; }
        private string PreviousHash { get; set; }
        private int Index { get; set; }


        public Block(DateTime times, string data, string previoushash, int index, int companyID, int productID)
        {
            Times = times;
            Data = data;
            Hash = calculateHash();
            PreviousHash = previoushash;
            Index = index;
            CompanyID = companyID;
            ProductID = productID;
        }

        public string calculateHash()
        {
            SHA256 sHA256 = SHA256.Create();
            byte[] input = Encoding.ASCII.GetBytes(Data + ProductID.ToString() + CompanyID.ToString() + DateTime.Now);
            byte[] output = sHA256.ComputeHash(input);

            return Convert.ToString(output);


        }
    }
}
