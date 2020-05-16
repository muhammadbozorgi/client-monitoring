using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace client
{
    class infosender
    {
        public static string Clientinfosender(string mac, string ip, string port)
        {
            float[] driveinfo = new float[100];
            int cputotal = 0;
            float ramtotal = 0;
            float[] totalRNET = new float[100];
            float[] totalSNET = new float[100];
            bool error = false;
            BsonDocument doc = new BsonDocument();
            doc.Clear();
            int i = 0;
            int b = 0;
            //NETWORK USAGE
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up))
            {
                totalRNET[i] = -(float)(ni.GetIPv4Statistics().BytesReceived) / (1024 * 1024);
                totalSNET[i] = -(float)(ni.GetIPv4Statistics().BytesSent) / (1024 * 1024);
                i++;
            }
            for (int c = 1; c != 0; c--)
            {
                cputotal += cpu();
                ramtotal += ram();
                Thread.Sleep(3000);
            }
            cputotal = cputotal / 6;
            ramtotal = ramtotal / 6;
            //NETWORK USAGE AGAIN FOR CALCULATE PER MIN
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up))
            {
                totalRNET[b] += (float)(ni.GetIPv4Statistics().BytesReceived) / (1024 * 1024);
                totalSNET[b] += (float)(ni.GetIPv4Statistics().BytesSent) / (1024 * 1024);
                if (totalRNET[b] != 0 || totalSNET[b] != 0)
                {
                    doc.Add(new BsonElement("Interface" + b, ni.Description));
                    doc.Add(new BsonElement("MBytes Sent for Interface " + b, totalSNET[b]));
                    doc.Add(new BsonElement("MBytes Rec for Interface" + b, totalRNET[b]));
                    if (totalRNET[b] > 100 || totalSNET[b] > 100)
                    {
                        error = true;
                    }
                }
                b++;
            }
            //GET DRIVETINFO
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                try
                {
                    if (drive.TotalFreeSpace != 0)
                    {
                        doc.Add(new BsonElement(drive.Name + "free space(GB): ", (drive.TotalFreeSpace) / 1e9));
                    }

                }
                catch { }
            }
            doc.Add(new BsonElement("total cpu usage: ", cputotal));
            doc.Add(new BsonElement("total free ram(MB): ", ramtotal));
            /////////////////////////////////////////////CHECK MY CONDITION FOR SEND DATA TO DATABASE OR NOT
            if (cputotal > 1 || ramtotal < 1000 || error)
            {
                //connect to mongo
                var dbClient = new MongoClient("mongodb://client:client99@" + ip + ":" + port + "/admin");
                ////create collection
                IMongoDatabase monitoringdatabase = dbClient.GetDatabase("monitoring");
                ////creat Mac object
                var monitoringcollection = monitoringdatabase.GetCollection<BsonDocument>(mac);
                //create bson
                monitoringcollection.InsertOne(doc);
                Console.WriteLine("send data");
                //nfosender.CreateTestMessage2(mac, doc);
                ////create collection
                IMongoDatabase commandsdatabase = dbClient.GetDatabase("commands");
                ////creat Mac object
                var commadscollection = commandsdatabase.GetCollection<BsonDocument>(mac);
                var filter = Builders<BsonDocument>.Filter.Eq("name", "server");
                var servercommand = commadscollection.Find(filter).FirstOrDefault();
                try
                {
                    string servercommand1 = servercommand.ElementAt(2).Value.ToString();
                    return servercommand1;
                }
                catch
                {
                    return null;

                }

            }
            else
            {
                var dbClient = new MongoClient("mongodb://client:client99@" + ip + ":" + port + "/admin");
                ////create collection
                IMongoDatabase commandsdatabase = dbClient.GetDatabase("commands");
                ////creat Mac object
                var commadscollection = commandsdatabase.GetCollection<BsonDocument>(mac);
                var filter = Builders<BsonDocument>.Filter.Eq("name", "server");
                var servercommand = commadscollection.Find(filter).FirstOrDefault();
                try
                {
                    string servercommand1= servercommand.ElementAt(2).Value.ToString();
                    return servercommand1;

                }
                catch
                {
                    return null;
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
        public static void CreateTestMessage2(string header, BsonDocument matn)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress("muhammadbozorgi@gmail.com");
                mail.To.Add("mohammadbozorgi0@gmail.com");
                mail.Subject = header;
                mail.Body = ("my mac: " + header + "," + matn.ToString()).Replace(",", Environment.NewLine);
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("muhammadbozorgi@gmail.com", "23676653");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                Console.WriteLine("mail Send");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}

