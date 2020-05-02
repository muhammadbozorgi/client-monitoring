using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ProcessAsyncStreamSamples
{
    class SortOutputRedirection
    {
        // Define static variables shared by class methods.
        private static StringBuilder sortOutput = null;
        private static StringBuilder sortOutput1 = null;

        public static void SortInputListText(string SERVER_IP, int port, string pass)
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
            byte[] bytesToRead1 = new byte[client.ReceiveBufferSize];
            int bytesRead1 = nwStream.Read(bytesToRead1, 0, client.ReceiveBufferSize);
            string password = Encoding.ASCII.GetString(bytesToRead1, 0, bytesRead1);
            if (password != pass)
            {
                byte[] outputbuf2 = ASCIIEncoding.ASCII.GetBytes("ur pass  incorrect");
                nwStream.Write(outputbuf2, 0, outputbuf2.Length);
                client.Close();
                listener.Stop();
                return;
            }
            byte[] outputbuf3 = ASCIIEncoding.ASCII.GetBytes("password correct");
            nwStream.Write(outputbuf3, 0, outputbuf3.Length);
            Process sortProcess = new Process();
            sortProcess.StartInfo.FileName = "/bin/bash";
            sortProcess.StartInfo.UseShellExecute = false;
            sortProcess.StartInfo.RedirectStandardOutput = true;
            sortProcess.StartInfo.RedirectStandardError = true;
            sortOutput = new StringBuilder();
            sortOutput1 = new StringBuilder();
            sortProcess.OutputDataReceived += SortOutputHandler;
            sortProcess.ErrorDataReceived += SortOutputHandler1;
            sortProcess.StartInfo.RedirectStandardInput = true;
            sortProcess.Start();

            var sortprocessid = sortProcess.Id;
            StreamWriter sortStreamWriter = sortProcess.StandardInput;
            sortProcess.BeginOutputReadLine();
            sortProcess.BeginErrorReadLine();
            String inputText;
            try
            {
                do
                {
                    byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                    int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                    Console.WriteLine("Received : " + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));
                    inputText = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                    if (inputText == "mykill" || inputText == "")
                    {
                        byte[] outputbuf2 = ASCIIEncoding.ASCII.GetBytes("good bye");
                        nwStream.Write(outputbuf2, 0, outputbuf2.Length);
                        sortStreamWriter.Close();
                        sortProcess.Kill();
                        sortProcess.Close();
                        client.Close();
                        listener.Stop();
                        break;
                    }
                    sortOutput.Clear();
                    sortOutput1.Clear();
                    try
                    {
                        sortStreamWriter.WriteLine(inputText);

                    }
                    catch
                    {
                        byte[] outputbuf = ASCIIEncoding.ASCII.GetBytes("good bye");
                        nwStream.Write(outputbuf, 0, outputbuf.Length);
                        sortStreamWriter.Close();
                        sortProcess.Kill();
                        sortProcess.Close();
                        client.Close();
                        listener.Stop();
                        break;
                    }
                    Thread.Sleep(1500);
                    if (!String.IsNullOrEmpty(sortOutput.ToString()))
                    {
                        byte[] outputbuf = ASCIIEncoding.ASCII.GetBytes(sortOutput.ToString());
                        nwStream.Write(outputbuf, 0, outputbuf.Length);
                    }
                    if (!String.IsNullOrEmpty(sortOutput1.ToString()))
                    {
                        Thread.Sleep(1500);
                        byte[] outputbuf = ASCIIEncoding.ASCII.GetBytes(sortOutput1.ToString());
                        nwStream.Write(outputbuf, 0, outputbuf.Length);
                    }
                    if (String.IsNullOrEmpty(sortOutput.ToString()) && String.IsNullOrEmpty(sortOutput1.ToString()))
                    {
                        byte[] outputbuf = ASCIIEncoding.ASCII.GetBytes("ur command havent any output");
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
        private static void SortOutputHandler1(object sendingProcess,
    DataReceivedEventArgs outLine)
        {
            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                sortOutput1.Append(Environment.NewLine +
                    $" {outLine.Data}");
            }
        }
    }
}

