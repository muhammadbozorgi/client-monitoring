using System;
using System.Threading;

public class SocketClient
{
    public static void Main(String[] args)
    {
        Console.WriteLine("please enter database ip:");
        string databaseip = "127.0.0.1";
        //Console.ReadLine();
        Console.WriteLine("please enter database port:");
        string databaseport = "50000";
        //Console.ReadLine();
        Console.WriteLine("please enter server ip:");
        string serverip = "127.0.0.1";
        // Console.ReadLine();
        Console.WriteLine("please enter server port:");
        string port = "5000";
        //Console.ReadLine();
        Thread t = new Thread(() => client.infosender.clientinfosender(databaseip, databaseport));
        t.Start();
        while (true)
        {
           
            try
            {
                ProcessAsyncStreamSamples.SortOutputRedirection.SortInputListText(serverip,port);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured in listenning to server: " + ex);

            }
        }

    }
}


