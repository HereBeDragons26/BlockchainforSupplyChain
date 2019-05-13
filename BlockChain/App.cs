using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Blockchain {
    public class App {

        public static void Main() {
            IPAddress[] address = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            TCP.myIP = address[1].ToString();
            Console.WriteLine("Your ip is: " + TCP.myIP + "\n" +
                "Is it true Y/N");
            string answer = Console.ReadLine();
            if(!answer.ToLower().Equals("y")) {
                Console.WriteLine("Enter your ip");
                TCP.myIP = Console.ReadLine();
            }
                
            TCP.StartListener();
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            //Test.TestMethod();
            Console.WriteLine("\nMiner application starting...\n");

            MainMethod();
        }

        public static void MainMethod() {
            string title = @"
   _____                   _        _____ _           _       
  / ____|                 | |      / ____| |         (_)      
 | (___  _   _ _ __  _ __ | |_   _| |    | |__   __ _ _ _ __  
  \___ \| | | | '_ \| '_ \| | | | | |    | '_ \ / _` | | '_ \ 
  ____) | |_| | |_) | |_) | | |_| | |____| | | | (_| | | | | |
 |_____/ \__,_| .__/| .__/|_|\__, |\_____|_| |_|\__,_|_|_| |_|
              | |   | |       __/ |                           
              |_|   |_|      |___/                            
";
            Console.WriteLine(title);
            Console.WriteLine(
                "Command List: \n" +
                "connect [ip] --> Connect to website and join other miners. [ip] is webserver's ip.\n" +
                "exit --> Leave the network\n" +
                "quit --> exit application\n");

            while (true) {
                string command = Console.ReadLine();
                command = command.ToLower();

                if (command.StartsWith("connect")) {
                    if (command.Length > 8)
                        command = command.Substring(8);
                    else {
                        Console.WriteLine("Please enter the ip");
                        continue;
                    }
                    if (!command.Contains(".")) {
                        Console.WriteLine("Please enter the ip");
                        continue;
                    }
                    TCP.WebServerIp = command;
                    Miners.ConnectToNetwork();
                    continue;
                }
                if (command.Equals("exit")) {
                    Console.WriteLine("Leaving network...\nPlease press a key");
                    Console.ReadKey();
                    Console.Clear();
                    MainMethod();
                    break;
                }
                if (command.Equals("quit")) {
                    Environment.Exit(0);
                }
            }
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e) {
            TCP.SendWebServer("exit");
            Console.WriteLine("\n\nExiting...");
            Console.WriteLine("Please press a key");
            Console.ReadKey();
        }

    }
}
