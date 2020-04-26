using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

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
                    string firstipAddress = string.Empty;
                    int sampleperminute;
                    bool error;
                    float[] driveinfo = new float[100];
                    int cputotal;
                    float ramtotal;
                    int sampleperminutecopy;
                    while (true)
                    {
                        float[] totalRNET = new float[100];
                        float[] totalSNET = new float[100];
                        //////////////////////////////////////////harchand saniye check konam bad befrestam?
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
                            if (totalRNET[b] != 0 || totalSNET[b] != 0 && ni.OperationalStatus == OperationalStatus.Up) 
                            {
                                if(ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback && ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                                    && ni.Name.StartsWith("vEthernet") == false && ni.Description.Contains("Hyper-v") == false)
                                {
                                    foreach (UnicastIPAddressInformation ip1 in ni.GetIPProperties().UnicastAddresses)
                                    {
                                        if (ip1.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                        {
                                            firstipAddress = ip1.Address.ToString();
                                            doc.Add(new BsonElement(firstipAddress+" MAC ", ni.GetPhysicalAddress().ToString()));
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
                            var dbClient = new MongoClient("mongodb://" + ip + ":" + port);
                            ////create collection
                            IMongoDatabase university = dbClient.GetDatabase("university");
                            ////creat Mac object
                            var IP = university.GetCollection<BsonDocument>(firstipAddress);
                            //create bson
                            IP.InsertOne(doc);
                        }
                        else
                        {
                            Console.WriteLine("good");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured in get data and sent to database: " + ex.GetType().ToString()+ ex);
                    
                }
            }
        }
    }
}
