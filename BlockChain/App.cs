﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Blockchain {
    public class App {

        public static void Main() {
            IPAddress[] address = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            List<string> addressList = new List<string>();
            int b = 1;
            for(int a = 0; a < address.Length; a++) {
                string ipString = address[a].ToString();
                if(!ipString.Contains("::")) {
                    addressList.Add(address[a].ToString());
                    Console.WriteLine(b + "- " + address[a].ToString());
                    b++;
                }
            }
            if(addressList.Count == 1) {
                TCP.myIP = addressList[0];
            }
            else {
                Console.WriteLine("Please select your ip.");
                int select = int.Parse(Console.ReadLine());
                TCP.myIP = addressList[select - 1];
            }
            Console.WriteLine("Your ip: " + TCP.myIP);
                
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
                "print [id] --> Print block which given id\n" + 
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
                if (command.StartsWith("print")) {
                    command = command.Substring(6);
                    Console.WriteLine(BlockChain.GetBlock(int.Parse(command)).ToString());
                    break;
                }
                if (command.StartsWith("chain")) {
                    Console.WriteLine("\n--------BLOCKCHAIN--------");
                    BlockChain.GetChain().ForEach(b => Console.WriteLine(b.ToString()));
                    Console.WriteLine("--------BLOCKCHAIN--------\n");
                }
                if (command.StartsWith("get")) {
                    if(BlockChain.GetChain().Count == 1) TCP.Send(Miners.minerIPs[0], "getChain");
                    else Console.WriteLine("Already have chain");
                }
                if (command.StartsWith("read")) {
                    string path = command.Substring(5);
                    string st = File.ReadAllText(path);
                    object ret = TCP.JsonDeserialize(st);
                    var obj = TCP.Cast(ret, new { list = new List<Block>() });
                    BlockChain.SetChain(obj.list);
                    TCP.SendWebServer("addMeNow");
                }
                if (command.StartsWith("write")) {
                    string path = command.Substring(6);
                    string st = TCP.JsonSerialize(new { list = BlockChain.GetChain() });
                    File.WriteAllText(path, st);
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
