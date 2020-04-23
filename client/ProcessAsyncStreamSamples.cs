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
        public static void SortInputListText(string serverip , string port)
        {
            
            int PORT_NO = Convert.ToInt32(port);
            string SERVER_IP = serverip;
            //---listen at the specified IP and port no.---
            IPAddress localAdd = IPAddress.Parse(SERVER_IP);
            TcpListener listener = new TcpListener(localAdd, PORT_NO);
            Console.WriteLine("Listening...");
            listener.Start();
            //---incoming client connected---
            TcpClient client = listener.AcceptTcpClient();
            //---get the incoming data through a network stream---
            NetworkStream nwStream = client.GetStream();
            Process sortProcess = new Process();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                sortProcess.StartInfo.FileName = "powershell.exe";

            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                sortProcess.StartInfo.FileName = "powershell.exe";
            }
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
            do
            {
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                Console.WriteLine("Received : " + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));
                inputText = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                sortOutput.Clear();
                if (!String.IsNullOrEmpty(inputText))
                {
                    sortStreamWriter.WriteLine(inputText);
                    Thread.Sleep(1500);
                    byte[] outputbuf = ASCIIEncoding.ASCII.GetBytes(sortOutput.ToString());
                    nwStream.Write(outputbuf, 0, outputbuf.Length);
                }
            }
            while (!String.IsNullOrEmpty(inputText) && !string.IsNullOrEmpty(sortOutput.ToString()));
            byte[] outputbuf1 = ASCIIEncoding.ASCII.GetBytes("good bye");
            nwStream.Write(outputbuf1, 0, outputbuf1.Length);
            // End the input stream to the sort command.
            sortStreamWriter.Close();
            // Wait for the sort process to write the sorted text lines.
            sortProcess.WaitForExit();
            sortProcess.Close();
            client.Close();
            listener.Stop();

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
