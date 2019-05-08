using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Blockchain {
    class Test {
        public static void Main(String[] args) {
            BlockChain.AddNewMiner("10.27.49.6");
            BlockChain.AddNewMiner("10.27.49.8");

            TCP.StartListener();

            Data data = new Data();
            data.ParentID.Add(12);
            data.Product = new Product();
            data.Product.Features.Add(new Feature(DateTime.Now, "buğday asdsa"));

            BlockChain.ReceiveNewBlock(data);
        }
    }
}
