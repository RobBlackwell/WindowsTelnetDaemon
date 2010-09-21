#region Copyright (c) 2010 Active Web Solutions Ltd
//
// (C) Copyright 2010 Active Web Solutions Ltd
//      All rights reserved.
//
// This software is provided "as is" without warranty of any kind,
// express or implied, including but not limited to warranties as to
// quality and fitness for a particular purpose. Active Web Solutions Ltd
// does not support the Software, nor does it warrant that the Software
// will meet your requirements or that the operation of the Software will
// be uninterrupted or error free or that any defects will be
// corrected. Nothing in this statement is intended to limit or exclude
// any liability for personal injury or death caused by the negligence of
// Active Web Solutions Ltd, its employees, contractors or agents.
//
#endregion

using System;
using System.Net;
using System.Reflection;

namespace TelnetD
{
    class Program
    {
        /// <summary>
        /// Get the version from the Assembly Information
        /// </summary>
        static string Version()
        {
            return new AssemblyName(Assembly.GetExecutingAssembly().FullName).Version.ToString();
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage: telnetd.exe <hostname> <port> [password]");
        }

        static void Main(string[] args)
        {
            if (args.Length < 2)
                PrintUsage();

            IPAddress address = IPAddress.Parse(args[0]);
            int port = int.Parse(args[1]);

            string password = null;
            if (args.Length > 2)
                password = args[2];

            Console.WriteLine("TelnetD {0}", Version());
            Console.WriteLine("Copyright (c) 2010 Active Web Solutions Ltd [www.aws.net]");

            Daemon daemon = new Daemon(address, port, password);
            daemon.Start();

        }
    }
}
