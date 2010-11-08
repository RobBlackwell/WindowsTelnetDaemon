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
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace TelnetD
{
    class TelnetServer
    {
        private TcpClient tcpClient;
        private Process process;
        private NetworkStream networkStream;
        private string password;

        public TelnetServer(TcpClient tcpClient, string password)
        {
            this.tcpClient = tcpClient;
            networkStream = tcpClient.GetStream();
            this.password = password;
        }


        public void CopyStream(StreamReader sr)
        {
            char[] buffer = new char[256];

            int i;
            while ((i = sr.Read(buffer, 0, buffer.Length)) > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(buffer);

                // TODO: See RFC854 and support IAC protocol etc

                networkStream.Write(bytes, 0, i);
                networkStream.Flush();
                
            }
        }

        /// <summary>
        /// Take standard output from the CMD process and write it to the network
        /// </summary>
        public void CopyStandardOutput()
        {
            CopyStream(process.StandardOutput);
        }

        /// <summary>
        /// Take error output from the CMD process and write it to the network
        /// </summary>
        public void CopyStandardError()
        {
            CopyStream(process.StandardError);
        }

        public void Banner()
        {
            StreamWriter streamWriter = new StreamWriter(networkStream);

            streamWriter.WriteLine("{0} {1}\n", Environment.OSVersion, Environment.MachineName);
            streamWriter.Flush();
        }

        public bool Login()
        {
            StreamWriter streamWriter = new StreamWriter(networkStream);
            StreamReader streamReader = new StreamReader(networkStream);

            for (int tries = 0; tries < 3; tries++)
            {
                streamWriter.Write("Password: ");
                streamWriter.Flush();
                string response = streamReader.ReadLine();
                if (response == password)
                {
                    streamWriter.WriteLine("\n");
                    streamWriter.Flush();
                    return true;
                }
            }

            return false;
        }

        public void Connect()
        {
            try
            {
                Banner();

                if ((password != null) && !Login())
                {
                    // Login failed
                    tcpClient.Close();
                    return;
                }

                // Start a new cmd.exe shell and connect up it's input and 
                // output to the TCPClient

                string command = "cmd.exe";

                ProcessStartInfo startInfo = new ProcessStartInfo(command)
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                process = new Process()
                {
                    StartInfo = startInfo
                };

                process.Start();

                Thread outputThread = new Thread(new ThreadStart(CopyStandardOutput));
                outputThread.Start();

                Thread errorThread = new Thread(new ThreadStart(CopyStandardError));
                errorThread.Start();

                byte[] buffer = new byte[256];

                const int CONTROL_D = 4;

                try
                {
                    int i;
                    while ((i = networkStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if ((i > 0) && (buffer[0] == CONTROL_D))
                        {
                            outputThread.Abort();
                            errorThread.Abort();
                            process.Close();
                            tcpClient.Close();

                            Console.WriteLine("Connection {0} closed", Thread.CurrentThread.ManagedThreadId);

                            return;
                        }
                        process.StandardInput.Write(Encoding.ASCII.GetChars(buffer), 0, i);
                    }
                }
                catch (Exception)
                {
                    // Clean up
                    outputThread.Abort();
                    errorThread.Abort();
                    process.Close();
                    tcpClient.Close();
                    throw;
                }

            }
            catch (Exception ex)
            {
                // Catch any errors on this thread, print them out, and shut down the thread so any 
                // other concurrent threads can continue without the process dying.
                Console.WriteLine("Error on Connection {0}", Thread.CurrentThread.ManagedThreadId);
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
