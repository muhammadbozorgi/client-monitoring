using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Driver;

public class SocketClient
{
    public static void clientinfosender(string ip, string port)
    {
        while (true)
        {

            try
            {
                string firstMacAddress = string.Empty;
                int sampleperminute;
                bool error;
                float[] driveinfo = new float[100];
                int cputotal;
                float ramtotal;
                int sampleperminutecopy;
                while (1 == 1)
                {
                    float[] totalRNET = new float[100];
                    float[] totalSNET = new float[100];
                    /////////////////////////////////////////////////////////////////////////////harchand saniye check konam bad befrestam?
                    sampleperminute = 10;
                    sampleperminutecopy = sampleperminute;
                    cputotal = 0;
                    ramtotal = 0;
                    error = false;
                    BsonDocument doc = new BsonDocument();
                    doc.Clear();
                    int i = 0;
                    int b = 0;
                    //NETWORK USAGE
                    foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        totalRNET[i] = -(float)(ni.GetIPv4Statistics().BytesReceived) / (1024 * 1024);
                        totalSNET[i] = -(float)(ni.GetIPv4Statistics().BytesSent) / (1024 * 1024);

                        i++;
                    }
                    //CPU AND RAM AVERAGE
                    while (sampleperminute != 0)
                    {
                        PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                        PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                        cpuCounter.NextValue();
                        ramCounter.NextValue();
                        System.Threading.Thread.Sleep(1000);
                        cputotal += (int)cpuCounter.NextValue();
                        ramtotal += ramCounter.NextValue();
                        sampleperminute--;
                    }
                    //NETWORK USAGE AGAIN FOR CALCULATE PER MIN
                    foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        totalRNET[b] += (float)(ni.GetIPv4Statistics().BytesReceived) / (1024 * 1024);
                        totalSNET[b] += (float)(ni.GetIPv4Statistics().BytesSent) / (1024 * 1024);
                        if (totalRNET[b] != 0 || totalSNET[b] != 0 && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        {
                            firstMacAddress = ni.GetPhysicalAddress().ToString();
                            doc.Add(new BsonElement("Interface" + b, ni.Description));
                            doc.Add(new BsonElement("MBytes Sent for Interface " + b, totalSNET[b]));
                            doc.Add(new BsonElement("MBytes Rec for Interface" + b, totalRNET[b]));
                            if (totalRNET[b] > 3 || totalSNET[b] > 3)
                            {
                                error = true;
                            }
                        }
                        b++;
                    }
                    //GET DRIVETINFO
                    foreach (DriveInfo drive in DriveInfo.GetDrives())
                    {
                        doc.Add(new BsonElement(drive.Name + "free space(GB): ", (drive.TotalFreeSpace) / 1e9));
                        if (((drive.TotalFreeSpace) / 1e9) < 1)
                        {
                            error = true;
                        }
                    }
                    cputotal = cputotal / sampleperminutecopy;
                    doc.Add(new BsonElement("total cpu usage: ", cputotal));
                    ramtotal = ramtotal / sampleperminutecopy;
                    doc.Add(new BsonElement("total free ram(MB): ", ramtotal));
                    /////////////////////////////////////////////CHECK MY CONDITION FOR SEND DATA TO DATABASE OR NOT
                    if (error || cputotal > 1 || ramtotal < 2000)
                    {
                        //connect to mongo
                        var dbClient = new MongoClient("mongodb://"+ip+":"+port);
                        ////create collection
                        IMongoDatabase university = dbClient.GetDatabase("university");
                        ////creat Mac object
                        var MAC = university.GetCollection<BsonDocument>(firstMacAddress);
                        //create bson
                        MAC.InsertOne(doc);
                    }
                    else
                    {
                        Console.WriteLine("good");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured in get data and sent to database: " + ex.GetType().ToString());
                break;
            }

        }


    }

    public static int Main(String[] args)
    {
        Console.WriteLine("please enter server ip:");
        string serverip = Console.ReadLine();
        Console.WriteLine("please enter server port:");
        string port = Console.ReadLine();
        Thread t = new Thread(() => clientinfosender(serverip,port));
        t.Start();
        // new Thread(clientinfosender("kjlk","hjk")).Start();
        while (true)
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

            while (true)
            {
                try
                {
                    byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                    int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                    Console.WriteLine("Received : " + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));
                    string command = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                    Console.WriteLine(command);
                    // Console.ReadLine();
                    if (command == "exit") // quit and close socket
                    {
                        string exit = "good bye";
                        byte[] exitbytes = ASCIIEncoding.ASCII.GetBytes(exit);
                        nwStream.Write(exitbytes, 0, exitbytes.Length);
                        listener.Stop();
                        break;
                    }
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        // execute command
                        Process p = new Process();
                        p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        p.StartInfo.FileName = "powershell.exe";
                        p.StartInfo.Arguments = "/C " + command;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.RedirectStandardError = true;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.Verb = "runas";
                        p.Start();

                        string output = p.StandardOutput.ReadToEnd();
                        string error = p.StandardError.ReadToEnd();

                        // sending command output
                        byte[] outputbuf = ASCIIEncoding.ASCII.GetBytes(output);
                        byte[] errorbuf = ASCIIEncoding.ASCII.GetBytes(error);
                        nwStream.Write(outputbuf, 0, outputbuf.Length);
                        nwStream.Write(errorbuf, 0, errorbuf.Length);
                        p.Close();
                    }
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {

                    }

                }
                catch
                {
                    listener.Stop();
                    break;
                }


            }
        }
    }
}


