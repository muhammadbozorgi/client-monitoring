using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;

public class SocketListener
{

    public static int Main(String[] args)
    {
        string dataReceived = string.Empty;
        while (true)
        {
            while (true)
            {
                try
                {
                    const int PORT_NO = 5000;
                    Console.WriteLine("enter ip of your client: ");
                    string SERVER_IP = Console.ReadLine();
                    //---create a TCPClient object at the IP and port no.---
                    TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
                    NetworkStream nwStream = client.GetStream();
                    Console.WriteLine("enter pass: ");
                    string textToSend1 = Console.ReadLine();
                    if (string.IsNullOrEmpty(textToSend1))
                    {
                        textToSend1 = " ";
                    }
                    byte[] bytesToSend1 = ASCIIEncoding.ASCII.GetBytes(textToSend1);
                    //---send the text---
                    Console.WriteLine("Sending : " + textToSend1);
                    nwStream.Write(bytesToSend1, 0, bytesToSend1.Length);
                    //---write back the text to the client---
                    byte[] buffer1 = new byte[client.ReceiveBufferSize];
                    //---read incoming stream---
                    int bytesRead1 = nwStream.Read(buffer1, 0, client.ReceiveBufferSize);
                    //---convert the data received into a string---
                    dataReceived = Encoding.ASCII.GetString(buffer1, 0, bytesRead1);
                    Console.WriteLine("Received : " + dataReceived);
                    if (dataReceived == "ur pass  incorrect")
                    {
                        client.Close();
                        break;
                    }
                    while (true)
                    {
                        //---write back the text to the client---
                        Console.WriteLine("enter your command: ");
                        string textToSend = Console.ReadLine();
                        if (string.IsNullOrEmpty(textToSend))
                        {
                            textToSend = " ";
                        }
                        byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);
                        //---send the text---
                        Console.WriteLine("Sending : " + textToSend);
                        nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                        byte[] buffer = new byte[client.ReceiveBufferSize];
                        //---read incoming stream---
                        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
                        //---convert the data received into a string---
                        dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Console.WriteLine("Received : " + dataReceived);
                        if (dataReceived == "good bye")
                        {
                            client.Close();
                            break;
                        }

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.GetType().ToString()+e);
                    break;
                }
            }

        }

    }
}