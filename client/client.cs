using System;
using System.Threading;

public class SocketClient
{
    public static void Main(String[] args)
    {
        while(true)
        {
            try
            {
                Console.WriteLine("please enter database ip:");
                string databaseip = "127.0.0.1";
                //Console.ReadLine();
                Console.WriteLine("please enter database port:");
                string databaseport = "27017";
                //Console.ReadLine();
                Console.WriteLine("please enter server ip:");
                string serverip = "127.0.0.1";
                // Console.ReadLine();
                Console.WriteLine("please enter server port:");
                string result = Console.ReadLine();
                int port = Int32.Parse(result);
                Thread t = new Thread(() => client.infosender.clientinfosender(databaseip, databaseport));
                t.Start();
                while (true)
                {
                    ProcessAsyncStreamSamples.SortOutputRedirection.SortInputListText(serverip, port);
                }
                
            }
            catch
            {
                
            }
        }
    }
}


