using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Blockchain {
    public class Test {
        public static void TestMethod() {
            IPAddress[] address = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            TCP.myIP = address[1].ToString();

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
