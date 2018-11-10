using GuptaMigration.Constants;
using GuptaMigration.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static GuptaMigration.Constants.ConstantsClass;

namespace GuptaMigration.Services
{
    public class DependencyParser
    {
        Dictionary<string, Dictionary<string,int>> FunctionsTablesDependency = new Dictionary<string, Dictionary<string, int>>();
        public List<RequestRecord> RequestRecords { get; set; }
        List<RequestRecord> RequestRecordsDupl = new List<RequestRecord>();
        public List<string> TableNames { get; set; }
        string FileName = "";
        string[] opeartions;
        HashSet<string> FunctionsNamesList = new HashSet<string>();

        public DependencyParser()
        {
            opeartions = new string[]{ ParsingConstants.InsertString,
                ParsingConstants.DeeteString, ParsingConstants.UpdateString};

            /*opeartions = new string[]{ ParsingConstants.InsertString,
                ParsingConstants.DeeteString, ParsingConstants.UpdateString,ParsingConstants.SelectString };*/
            RequestRecords = new List<RequestRecord>();
        }

        public void ParseDocument(string fileName,string file)
        {
            //RequestRecords = new List<RequestRecord>();       
            FileName = fileName;
            foreach (var op in opeartions)
            {
                FindTableOperations(op,file);        
            }            
        }

        public void FindTableOperations(string operation, string file)
        {
            //if we see the insert statement, we can drop everything after and finf the last function in the file

            string currentFile = file;

            while (currentFile.Contains(operation))
            {
                

                string functionFullText = "";              

                CutFileOnCurrentFunctionAndRest(ref currentFile, operation,
                    ref functionFullText);

                //Get function name   
                //when the rest of the file does not have the function anymore ? but how could that be...
                string FunctionName = "";

                if (!functionFullText.ToLower().Contains("function:"))
                {
                    FunctionName = FileName;
                }
                else
                {
                    FunctionName = GetFunctionName(functionFullText);
                }
                //Get all sql operation calls in current function
                List<string> tableNames = GetTableNames(functionFullText, operation);
                LogDuplicates(tableNames, FunctionName, operation, FileName);
                foreach (var table in tableNames)
                {
                    var record = RequestRecords.Where(r => r.FunctionName == FunctionName & r.TableName == table).FirstOrDefault();
                    

                    if (record != null)
                    {
                        if(!record.Operation.Contains(operation))
                        {
                            record.Operation.Add(operation);
                            if( operation != ParsingConstants.SelectString)
                            {
                                record.OperationLevel = 2;
                            }
                        }
                        
                        record.Count++;

                    }
                    else
                    {
                        RequestRecord recordreq = new RequestRecord
                        {
                            FunctionName = FunctionName.ToLower(),
                            Operation = new List<string> { operation },
                            TableName = table.ToLower(),
                            Count = 1,
                            OperationLevel = operation == ParsingConstants.SelectString ? 1 :2
                        };   
                        
                        RequestRecords.Add(recordreq);

                        FunctionsNamesList.Add(FunctionName.ToLower());
                    }
                }               
             
            }          

        }

        public void FindFunctionCalls(string fileName, string file)
        {
            string currentFile = file;

            while (currentFile.Contains("Call"))
            {

                int indexOfCall = currentFile.IndexOf("Call");
                var indexOfFunctionEnd = currentFile.IndexOf("(", indexOfCall);

                var FunctionCallString = currentFile.Substring(indexOfCall, indexOfFunctionEnd - indexOfCall);                
                var FunctionCallStringAr = FunctionCallString.Split(" ");

                if (FunctionCallStringAr.Count() > 1)
                {
                    string FunctionCalled = FunctionCallStringAr[1];

                    if (FunctionsNamesList.Contains(FunctionCalled.ToLower()))
                    {
                        string functionFullText = "";

                        CutFileOnCurrentFunctionAndRest(ref currentFile, FunctionCallString,
                            ref functionFullText);

                        //Get function name   
                        //when the rest of the file does not have the function anymore ? but how could that be...
                        string FunctionName = "";

                        if (!functionFullText.ToLower().Contains("function:"))
                        {
                            FunctionName = FileName.ToLower();
                        }
                        else
                        {
                            FunctionName = GetFunctionName(functionFullText);
                        }

                        var record = RequestRecords.Where(r => r.FunctionName == FunctionCalled & r.CalledByFunction == FunctionName)
                        .FirstOrDefault();


                        if (record == null)
                        {
                            RequestRecord recordreq = new RequestRecord
                            {
                                FunctionName = FunctionCalled,
                                Operation = new List<string> { "FunctionCall" },
                                CalledByFunction = FunctionName,
                                Count = 1,
                                OperationLevel = 1
                            };

                            RequestRecords.Add(recordreq);


                        }
                    }
                    else
                    {
                        currentFile = currentFile.Substring(indexOfFunctionEnd);
                    }
                }
                else
                {
                    currentFile = currentFile.Substring(indexOfFunctionEnd);

                }

            }

        }

        public void LogDuplicates(List<string> tableNames, string FunctionName, string operation, string fileName)
        {
            using (StreamWriter file =
            new StreamWriter(@"logs.txt"))
            {
                foreach (var table in tableNames)
                {
                    var record = RequestRecordsDupl.Where(r => r.FunctionName == FunctionName & r.TableName == table).FirstOrDefault();

                    if (record != null)
                    {
                        if (!record.Operation.Contains(operation))
                        {
                            record.Operation.Add(operation);
                            if (operation != ParsingConstants.SelectString)
                            {
                                record.OperationLevel = 2;
                            }
                        }

                        record.Count++;
                        file.WriteLine("Table {0} : , Function : {1} , Operation : {2} , FileName : {3} ", record.TableName, record.FunctionName, record.Operation, record.File);
                 
                        file.WriteLine("Table {0} : , Function : {1} , Operation : {2} , FileName : {3} ", table,  FunctionName,  operation,  fileName);
                    }
                    else
                    {
                        RequestRecord recordreq = new RequestRecord
                        {
                            FunctionName = FunctionName,
                            Operation = new List<string> { operation },
                            TableName = table,
                            Count = 1,
                            OperationLevel = operation == ParsingConstants.SelectString ? 1 : 2,
                            File = fileName
                        };
                        RequestRecordsDupl.Add(recordreq);
                    }
                    
                }
            }
        }

        public List<string> GetTableName(string statement, string operation)
        {
            List <string> tableNames = new List<string>();
            string[] words = statement.Split();

            if (operation == ParsingConstants.SelectString)
            {
                int start = statement.ToLower().IndexOf("join");
                
                    for(int i = 0; i< words.Length;i++)
                    {
                        if (words[i].ToLower() == "from")
                        {
                        if (TableNames.Contains(words[i + 1].ToLower()))
                        {
                            tableNames.Add(words[i + 1]);
                        }

                        }

                        if (words[i].ToLower() == "join")
                        {
                        if (TableNames.Contains(words[i + 1].ToLower()))
                        {
                            tableNames.Add(words[i + 1]);
                        }

                        }

                    }                
                
            }                       
            else{
                int EndOfTableNameIndex;

                string tableRaw = "";
                if (operation == ParsingConstants.UpdateString)
                {
                    tableRaw = words[1];
                }
                else
                {
                    tableRaw = words[2];
                }
                    EndOfTableNameIndex = tableRaw.IndexOf('(');
                
                if (EndOfTableNameIndex != -1)
                {
                    var table = tableRaw.Substring(0, EndOfTableNameIndex).Trim();
                    if (TableNames.Contains(table))
                    {
                        tableNames.Add(table.ToLower());
                    }
                }
                else
                {
                    var table = tableRaw.Trim();

                    if (TableNames.Contains(table.ToLower()))
                    {
                        tableNames.Add(table);
                    }
                }
            }
            return tableNames;
        }

        public string GetFunctionName(string functionText)
        {
            var functionStatementEndIndex = functionText.IndexOf(ConstantsClass.ParsingConstants.Head);
            //Get only the line with the function name 
            var functionLineText = functionText.Substring(0, functionStatementEndIndex);
            var functionLineArray = functionLineText.Split(" ");
            return functionLineArray[1].Replace("\n", "").Replace("\r", "")
                .Replace(":","").Replace("\t", "").Replace("!","").Replace("\"","");
        }

        public List<string> GetTableNames(string functionFullText, string operation)
        {
            int lineNr = functionFullText.IndexOf(operation);

            List<string> tableNames = new List<string>();

            if (lineNr == -1)
            {
                return null;
            }
            else
            {
                while (functionFullText.IndexOf(operation) != -1)
                {
                    lineNr = functionFullText.IndexOf(operation);
                    //The next line after found SQL operation
                    var cutlineNr = functionFullText.IndexOf(ConstantsClass.ParsingConstants.Head, lineNr);
                    
                    //Not the end of the function            
                    if ( cutlineNr != -1 )
                    {
                        var operationLineText = functionFullText.Substring(lineNr, cutlineNr - lineNr);
                        var tables = GetTableName(operationLineText, operation);
                        foreach (var t in tables)
                        {
                            if (!tableNames.Contains(t))
                            {
                                tableNames.Add(t);
                            }
                        }
                        
                        functionFullText = functionFullText.Substring(cutlineNr);
                    }
                    else
                    {
                        var tables = GetTableName(functionFullText, operation);
                        tableNames.AddRange(tables);
                        functionFullText = "";
                        break;

                    }
                }
                return tableNames;
            }
        }

        /// <summary>
        /// Find the next function with operation and devide the text on the current funtion and the rest
        /// </summary>
        /// <param name="currentFile"></param>
        /// <param name="operation"></param>
        /// <param name="functionFullText"></param>
        public void CutFileOnCurrentFunctionAndRest(ref string currentFile, string operation,
            ref string functionFullText)
        {
            //The first found SQL operation
            var lineNr = currentFile.IndexOf(operation);

            if (lineNr == -1)
            {
                currentFile = "";
                functionFullText = "";
                //operationLineText = "";
            }
            else
            {
                //The next line after found SQL operation
               // var cutlineNr = currentFile.IndexOf(ConstantsClass.ParsingConstants.Head, lineNr);
                //Get next function index
                var nextFunctionIndex = currentFile.IndexOf("function:", lineNr);
                //Get everything from the beginning till the next line after found sql operation

                if (nextFunctionIndex != -1)
                {
                    var functionsBeforeIncludingCurrent = currentFile.Substring(0, nextFunctionIndex);
                    var startFunctionIndex = functionsBeforeIncludingCurrent.LastIndexOf("function:");

                    if (startFunctionIndex == -1)
                    {                        
                        functionFullText = functionsBeforeIncludingCurrent;
                    }
                    else
                    {
                        functionFullText = functionsBeforeIncludingCurrent.Substring(startFunctionIndex);
                    }
                    currentFile = currentFile.Substring(nextFunctionIndex);
                    //operationLineText = currentFile.Substring(lineNr, cutlineNr - lineNr);
                }
                else
                {
                    var startFunctionIndex = currentFile.LastIndexOf("function:");
                    if (startFunctionIndex == -1)
                    {
                        functionFullText = currentFile;
                        
                    }
                    else
                    {
                        functionFullText = currentFile.Substring(startFunctionIndex);
                    }
                    //operationLineText = currentFile.Substring(lineNr);
                    currentFile = "";
                }

            }

            if (functionFullText == "")
            {

                var a = 0;
            }
        }

        public List<RequestRecord> DefineFunctionCalls(List<RequestRecord> dependencies, string FileName)
        {
            return dependencies;
        }
    }
}


    

