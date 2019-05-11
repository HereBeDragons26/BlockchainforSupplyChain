using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Blockchain {
    class Test {
        public static void Main(String[] args) {
            IPAddress[] address = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            BlockChain.myIP = address[1].ToString();

            TCP.StartListener();

            BlockChain.ConnectToNetwork();

            //Data data = new Data();
            //data.ParentID.Add(12);
            //data.Product = new Product();
            //data.Product.Features.Add(new Feature(DateTime.Now, "buğday asdsa"));

            //BlockChain.ReceiveNewBlock(data);
        }
    }
}
