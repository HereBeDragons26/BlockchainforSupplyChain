using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain
{
  public  class BlockChain
    {
        public IList<Block> chain { get; set; }

        public BlockChain()
        {
            InitializeChain();
        }
        public void InitializeChain()
        {
            chain = new List<Block>();
        }

        public Block CreateBlock(string data, string previoushash, int index, int productID, int companyID)
        {
            return new Block(DateTime.Now, data, previoushash, index, productID, companyID);
        }

    }
}
