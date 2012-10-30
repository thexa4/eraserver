﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceProtocol;
using MongoDB.Driver;

namespace ResourceService
{
    class Program
    {
        static ServiceClient _erasClient;

        /// <summary>
        /// Server reference
        /// </summary>
        public static MongoServer Server { get; protected set; }

        /// <summary>
        /// Database reference
        /// </summary>
        public static MongoDatabase Database { get; protected set; }

        public static NetworkInfo NetworkInfo { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            System.Threading.Thread.Sleep(4000);

            _erasClient = ServiceClient.Connect("Resource");
            Console.WriteLine("my id is:" + _erasClient.ServiceName);
            
            // use message handlers to register what should be done with different msgtypes
            var q = _erasClient.CreateQuestion(MessageType.EraS, "Self");
            q.Packet.Write("mongo");
            var a = _erasClient.AskReliableQuestion(q); 

            NetworkInfo = new ServiceProtocol.NetworkInfo(_erasClient);

            // Example question
            Console.WriteLine(NetworkInfo.GetServerIdentifier());

            // TODO: get mongo url from EraS
            Server = MongoServer.Create("mongodb://pegu.maxmaton.nl");
            Database = Server.GetDatabase("era");

            System.Threading.Thread.Sleep(1000 * 10);
        }
    }
}
