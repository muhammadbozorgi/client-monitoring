using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace client
{
    class infosender
    {
        private static StringBuilder sortOutput = null;
        private static void SortOutputHandler(object sendingProcess,
          DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                sortOutput.Append(Environment.NewLine +
                    $"{outLine.Data}");
            }
        }
        public static void clientinfosender(string ip, string port)
        {
            while (true)
            {

                try
                {
                    Process sortProcess = new Process();
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        sortProcess.StartInfo.FileName = "cmd.exe";
                    }
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        sortProcess.StartInfo.FileName = "/bin/bash";
                    }
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        sortProcess.StartInfo.FileName = "/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
                    }
                    sortProcess.StartInfo.UseShellExecute = false;
                    sortProcess.StartInfo.RedirectStandardOutput = true;
                    sortOutput = new StringBuilder();
                    sortProcess.OutputDataReceived += SortOutputHandler;
                    sortProcess.StartInfo.RedirectStandardInput = true;
                    sortProcess.Start();
                    StreamWriter sortStreamWriter = sortProcess.StandardInput;
                    sortProcess.BeginOutputReadLine();
                    string firstipAddress = "cant find ip";
                    string macadd = "cant find mac";
                    bool error;
                    float[] driveinfo = new float[100];
                    int cputotal;
                    float ramtotal;
                    while (true)
                    {
                        float[] totalRNET = new float[100];
                        float[] totalSNET = new float[100];
                        //////////////////////////////////////////harchand saniye check konam bad befrestam?
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
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            sortOutput.Clear();
                            sortStreamWriter.WriteLine("(Get-WmiObject Win32_Processor).LoadPercentage");
                            sortOutput.Clear();
                            Thread.Sleep(5000);
                            Console.WriteLine(sortOutput);
                            string a = sortOutput.ToString();
                            Console.WriteLine(a);
                            cputotal = Convert.ToInt32(a);
                            sortStreamWriter.WriteLine("Get-WMIObject -class win32_ComputerSystem |  %{$_.TotalPhysicalMemory/(1024*1024)}");
                            ramtotal = Convert.ToInt32(sortOutput);
                            sortStreamWriter.WriteLine("Get-CIMInstance Win32_OperatingSystem | Select FreePhysicalMemory | %{$_.FreePhysicalMemory/1000}");
                            ramtotal = (ramtotal-Convert.ToInt32(sortOutput))/ramtotal;
                            Console.WriteLine(ramtotal);
                        }
                        else
                        {
                            sortStreamWriter.WriteLine("Get-WmiObject Win32_Processor | Select LoadPercentage | Format-List");
                           // cputotal = sortOutput;
                            sortStreamWriter.WriteLine("Get-WMIObject Win32_PhysicalMemory | Measure -Property capacity -Sum | %{$_.sum/1Mb}");
                            //ramtotal = sortOutput;
                        }
                        //NETWORK USAGE AGAIN FOR CALCULATE PER MIN
                        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                        {
                            totalRNET[b] += (float)(ni.GetIPv4Statistics().BytesReceived) / (1024 * 1024);
                            totalSNET[b] += (float)(ni.GetIPv4Statistics().BytesSent) / (1024 * 1024);
                            if (totalRNET[b] != 0 || totalSNET[b] != 0 && ni.OperationalStatus == OperationalStatus.Up)
                            {
                                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback && ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                                    && ni.Name.StartsWith("vEthernet") == false && ni.Description.Contains("Hyper-v") == false)
                                {
                                    foreach (UnicastIPAddressInformation ip1 in ni.GetIPProperties().UnicastAddresses)
                                    {
                                        if (ip1.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                        {
                                            firstipAddress = ip1.Address.ToString();
                                            macadd = ni.GetPhysicalAddress().ToString();
                                        }
                                    }
                                }
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
                        doc.Add(new BsonElement("MAC", macadd));
                        //GET DRIVETINFO
                        foreach (DriveInfo drive in DriveInfo.GetDrives())
                        {
                            try
                            {
                                doc.Add(new BsonElement(drive.Name + "free space(GB): ", (drive.TotalFreeSpace) / 1e9));

                                if (((drive.TotalFreeSpace) / 1e9) < 1)
                                {
                                    error = true;
                                }
                            }
                            catch { }
                        }
                        doc.Add(new BsonElement("total cpu usage: ", cputotal));
                        doc.Add(new BsonElement("total free ram(MB): ", ramtotal));
                        /////////////////////////////////////////////CHECK MY CONDITION FOR SEND DATA TO DATABASE OR NOT
                        if (error || cputotal > 1 || ramtotal < 2000)
                        {
                            //connect to mongo
                            var dbClient = new MongoClient("mongodb://" + ip + ":" + port);
                            ////create collection
                            IMongoDatabase university = dbClient.GetDatabase("university");
                            ////creat Mac object
                            var IP = university.GetCollection<BsonDocument>(firstipAddress);
                            //create bson
                            IP.InsertOne(doc);
                            Console.WriteLine("send data");
                        }
                        else
                        {
                            Console.WriteLine("good");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured in get data and sent to database: " + ex.GetType().ToString() + ex);

                }
            }
        }
    }
}
