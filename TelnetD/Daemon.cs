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
using System.Net.Sockets;
using System.Threading;

namespace TelnetD
{
    public class Daemon
    {
        private IPAddress address;
        private int port;
        private string password;

        public Daemon(IPAddress address, int port, string password)
        {
            this.address = address;
            this.port = port;
            this.password = password;
        }

        public void Start()
        {
            TcpListener tcpListener = new TcpListener(address, port);

            tcpListener.Start();

            // Keep processing inbound connections ..
            while (true)
            {
                Console.WriteLine("Waiting for a connection on {0}, port {1} ... ", address ,port);

                TcpClient tcpClient = tcpListener.AcceptTcpClient();

                TelnetServer telnetServer = new TelnetServer(tcpClient, password);

                Thread backgroundThread = new Thread(new ThreadStart(telnetServer.Connect));
                backgroundThread.Start();

                Console.WriteLine("Connected {0}", backgroundThread.ManagedThreadId);
            }
        }
    }
}
