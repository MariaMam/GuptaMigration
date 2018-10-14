using GuptaMigration.Constants;
using GuptaMigration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static GuptaMigration.Constants.ConstantsClass;

namespace GuptaMigration.Services
{
    public class DependencyParser
    {
        Dictionary<string, Dictionary<string,int>> FunctionsTablesDependency = new Dictionary<string, Dictionary<string, int>>();
        List<RequestRecord> RequestRecords = new List<RequestRecord>();
        public List<string> TableNames { get; set; }


        public List<RequestRecord> ParseDocument(string file,string fileName)
        {
            string[] opeartions = { ParsingConstants.InsertString,
                ParsingConstants.DeeteString, ParsingConstants.UpdateString,ParsingConstants.SelectString };

            foreach (var op in opeartions)
            {
                FindTableOperations(op,file, fileName);

            }

            return RequestRecords;
        }

            public List<RequestRecord> FindTableOperations(string operation, string file, string fileName)
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
                    FunctionName = fileName;
                }
                else
                {
                    FunctionName = GetFunctionName(functionFullText);
                }
                //Get all sql operation calls in current function
                List<string> tableNames = GetTableNames(functionFullText, operation);
                
                foreach(var table in tableNames)
                {
                    var record = RequestRecords.Where(r => r.FunctionName == FunctionName & r.TableName == table &&
                    r.Operation == operation).FirstOrDefault();

                    if (record != null)
                    {
                        record.Count++;

                    }
                    else
                    {
                        RequestRecord recordreq = new RequestRecord();
                        recordreq.FunctionName = FunctionName;
                        recordreq.Operation = operation;
                        recordreq.TableName = table;
                        recordreq.Count = 1;

                        RequestRecords.Add(recordreq);

                    }

                }

                   
             
            }

            return RequestRecords;

        }

        public List<string> GetTableName(string statement, string operation)
        {
            List <string> tableNames = new List<string>();
            string[] words = statement.Split();

            if (operation == ParsingConstants.SelectString)
            {
                int start = statement.IndexOf("JOIN");
                
                    for(int i = 0; i< words.Length;i++)
                    {
                        if (words[i].ToLower() == "from")
                        {
                        if (TableNames.Contains(words[i + 1].ToUpper()))
                        {
                            tableNames.Add(words[i + 1]);
                        }

                        }

                        if (words[i].ToLower() == "join")
                        {
                        if (TableNames.Contains(words[i + 1].ToUpper()))
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
                        tableNames.Add(table.ToUpper());
                    }
                }
                else
                {
                    var table = tableRaw.Trim();

                    if (TableNames.Contains(table.ToUpper()))
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
            return functionLineArray[1].Replace("\n", "").Replace("\r", "").Replace(":","");
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
                        tableNames.AddRange(tables);
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
                    functionFullText = currentFile;
                    currentFile = "";
                    //operationLineText = currentFile.Substring(lineNr);

                }

            }

            if (functionFullText == "")
            {

                var a = 0;
            }
        }
    }
}


    

