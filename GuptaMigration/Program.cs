using GuptaMigration.Models;
using GuptaMigration.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace GuptaMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            var files  = FileReader.ReadGuptaFiles();  
            
            DependencyParser parser = new DependencyParser();
            FileReader f = new FileReader();
            var tableNames = f.ReadTableNames();
            parser.TableNames = tableNames;          

            foreach (var file in files)
            {
               parser.ParseDocument(file.Item1,file.Item2.ToLower());
            }



            foreach (var file in files)
            {
                Console.WriteLine(file.Item1);
                if (file.Item1 == "patimp")
                {
                    Console.WriteLine("Attention");
                    parser.FindFunctionCalls(file.Item1, file.Item2.ToLower());
                }
                else
                {
                    parser.FindFunctionCalls(file.Item1, file.Item2.ToLower());
                }
            }

            List<RequestRecord> records = parser.RequestRecords;

            WriteToFile(records);

            Console.WriteLine(@"Sucessfully parsed {0} files", files.Count);
            Console.ReadLine();
        }

        public static void WriteToFile(List<RequestRecord> records)
        {
            Dictionary<string, string> NodeNames = new Dictionary<string, string>();

            int NodeNr=0;

        
            using (StreamWriter file =
            new StreamWriter(@"graphWrite.gml"))
            {

                string Nodes = "graph [ \n directed 0";
                string Edges = "";
                int n = 1;
                    foreach (var rec in records)
                {
                    // If the line doesn't contain the word 'Second', write the line to the file.
                    string Node1Nmr = "";
                    string Node2Nmr = "";

                    if(rec.FunctionName.Contains("preparetemp\"") || 
                         rec.CalledByFunction == "preparetemp\"")
                    {
                        var b = 0;
                    }

                        if (NodeNames.ContainsKey(rec.FunctionName))
                    {
                        Node1Nmr = NodeNames[rec.FunctionName];                       
                    }
                    else
                    {
                        Node1Nmr = NodeNr + "";
                        NodeNr++;
                        NodeNames[rec.FunctionName] = Node1Nmr;

                        string s = "\n node\n [\n  id " + Node1Nmr + "\n  label \"" + rec.FunctionName + "{F}\"\n ]";
                        Nodes += s;
                    }

                    var secondEdgeName = "";
                    var secondEdgeType = "{T}";

                    if (rec.TableName == null )
                    {
                        secondEdgeName = rec.CalledByFunction;
                        secondEdgeType = "{F}";
                    }
                    else
                    {
                        secondEdgeName = rec.TableName;
                    }
                
                    if (NodeNames.ContainsKey(secondEdgeName ))
                    {
                        Node2Nmr = NodeNames[secondEdgeName];
                    }
                    else
                    {
                        Node2Nmr = NodeNr + "";
                        NodeNames[secondEdgeName] = Node2Nmr;
                        Nodes += "\n node \n [\n  id " + Node2Nmr + "\n  label \"" + secondEdgeName + secondEdgeType+ "\"\n ]";
                        NodeNr++;
                    }
               
                    Edges += "\n edge "+"\n [ \n  source " + Node1Nmr + "\n  target " + Node2Nmr + "\n" +
                        "  weight " + (1.0 / rec.OperationLevel)+ "\n ]";
                    n++;

                    //file.WriteLine(Node1Nmr + " " + Node2Nmr + " " + rec.OperationLevel);
                    //file.WriteLine(rec.FunctionName + " "+ rec.TableName+ " "+ Node1Nmr + " " + Node2Nmr + " "+rec.OperationLevel);
                }

                file.WriteLine(Nodes);
                file.WriteLine(Edges);

                file.WriteLine("]");

            }


        }
    }
}
