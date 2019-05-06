using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain {
    class Test {
        public static void Main(String[] args) {
            TCP.Send("deneme");
            Console.WriteLine("deneme mesajı yollandı");
        }
    }
}
