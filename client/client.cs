using System;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;

public class SocketClient
{
    public static void Main(String[] args)
    {
        string databaseip;
        string databaseport = "27017";
        string serverip;
        string result;
        int port;
        while (true)
        {
            try
            {


                Console.WriteLine("please enter database ip:");
                databaseip = Console.ReadLine();
                Ping p1 = new Ping();
                PingReply PR = p1.Send(databaseip);
                Console.WriteLine("please enter ip that you want get menager command :");
                serverip = Console.ReadLine();
                // check when the ping is not success
                if (PR.Status.ToString().Equals("Success"))
                {
                    Console.WriteLine("ping database ip is true");
                    Console.WriteLine("please enter server port:");
                    result = Console.ReadLine();
                    port = Int32.Parse(result);
                    break;

                }
                Console.WriteLine("cant see database ip try again");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured : " + ex.GetType().ToString());

            }
        }

        Thread t = new Thread(() => client.infosender.clientinfosender(databaseip, databaseport));
        t.Start();
        while (true)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    ProcessAsyncStreamSamples.SortOutputRedirection1.SortInputListText(serverip, port);
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    ProcessAsyncStreamSamples.SortOutputRedirection.SortInputListText(serverip, port);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured in get data and sent to database: " + ex.GetType().ToString());

            }
        }

    }
}


