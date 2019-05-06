using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Blockchain {
    class Test {
        public static void Main(String[] args) {

            //Console.WriteLine(Dns.GetHostByName(Dns.GetHostName()).AddressList[1].ToString());
            //return;

            //TCP.Send("deneme");
            //TCP.StartListener();
            //string cmd;
            //while(true) {
            //    cmd = Console.ReadLine();
            //    if (cmd.Equals("q")) break;
            //    TCP.Send(cmd);
            //}
            //Console.ReadLine();

            BlockChain.CreateBlock(new List<long>(), new Product());
            Block block = BlockChain.GetChain()[1];
            
            if (block.Mine())
                Console.WriteLine(block.Nonce);
            else
                Console.WriteLine("fail");

            Console.WriteLine(block.Hash);
        }
    }
}
