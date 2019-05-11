using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain {
    public class App {

        public static void Main() {
            Test.TestMethod();

            //MainMethod();

        }

        public static void MainMethod() {
            Console.WriteLine(
                "Command List: \n" +
                "connectToNetwork [ip] --> Connect to website and join other miners. [ip] is webserver's ip. " +
                "");
        }
    }
}
