using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;

// Socket Listener acts as a server and listens to the incoming   
// messages on the specified port and protocol.  
public class SocketListener
{

    public static int Main(String[] args)
    {
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
                    while (true)
                    {
                        //---write back the text to the client---
                        Console.WriteLine("enter your command: ");
                        string textToSend = Console.ReadLine();
                        byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);
                        //---send the text---
                        Console.WriteLine("Sending : " + textToSend);
                        nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                        byte[] buffer = new byte[client.ReceiveBufferSize];
                        //---read incoming stream---
                        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
                        //---convert the data received into a string---
                        string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Console.WriteLine("Received : " + dataReceived);
                        if (dataReceived == "good bye")
                        {
                            client.Close();
                            break;
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.GetType().ToString());
                    break;
                }
            }

        }

    }
}