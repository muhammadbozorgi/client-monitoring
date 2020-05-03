﻿using MongoDB.Bson;
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
        public static void clientinfosender(string ip, string port)
        {
            while (true)
            {

                try
                {
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
                        for(int c =6; c!=0;c-- )
                        {
                            cputotal += cpu();
                            ramtotal += ram();
                            Thread.Sleep(10000);
                        }
                        cputotal = cputotal/6;
                        ramtotal = ramtotal/6;
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
                                if (totalRNET[b] > 1 || totalSNET[b] > 1)
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
                                if(drive.TotalFreeSpace !=0 )
                                {
                                    doc.Add(new BsonElement(drive.Name + "free space(GB): ", (drive.TotalFreeSpace) / 1e9));

                                    if (((drive.TotalFreeSpace) / 1e6) < 100)
                                    {
                                        error = true;
                                    }
                                }

                            }
                            catch { }
                        }
                        doc.Add(new BsonElement("total cpu usage: ", cputotal));
                        doc.Add(new BsonElement("total free ram(MB): ", ramtotal));
                        /////////////////////////////////////////////CHECK MY CONDITION FOR SEND DATA TO DATABASE OR NOT
                        if (error || cputotal > 60 || ramtotal < 1000)
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
                            Console.WriteLine("system is stable");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured in get data and sent to database: " + ex.GetType().ToString() );

                }
            }
        }
        public static int cpu()
        {
            int cputotal;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "(Get-CimInstance -Class Win32_Processor).LoadPercentage",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                proc.Start();
                string output = proc.StandardOutput.ReadToEnd();
                cputotal = (int)Convert.ToDouble(output);
                proc.Close();
            }
            else
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    proc.StartInfo.Arguments = "-c \" ps -A -o %cpu | awk '{s+=$1} END {print s}'\"";
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    proc.StartInfo.Arguments = "-c \" " + "grep 'cpu ' /proc/stat | awk '{usage=($2+$4)*100/($2+$4+$5)} END {print usage}'" + "\"";
                }
                proc.Start();
                string output = proc.StandardOutput.ReadToEnd();
                cputotal = (int)Convert.ToDouble(output);
                proc.Close();
            }
            return cputotal;
        }
        public static int ram()
        {
            int ramtotal;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var proc1 = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "Get-CIMInstance Win32_OperatingSystem | Select FreePhysicalMemory|%{$_.FreePhysicalMemory/1024}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                proc1.Start();
                string output1 = proc1.StandardOutput.ReadToEnd();
                ramtotal = (int)Convert.ToDouble(output1);
                proc1.Close();
            }
            else
            {
                var proc1 = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    proc1.StartInfo.Arguments = "-c \" top -l1 | awk '/PhysMem/ {print int($6)}'\"";
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    proc1.StartInfo.Arguments = "-c \" " + "free | awk 'FNR == 3 {print$4/1024}'" + "\"";
                }
                proc1.Start();
                string output1 = proc1.StandardOutput.ReadToEnd();
                ramtotal = (int)Convert.ToDouble(output1);
                proc1.Close();
            }
            return ramtotal;
        }
    }
}
